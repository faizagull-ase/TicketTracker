# TicketFlow.Api (Web API)

A REST API for the same support-ticket tracker as the [TicketFlow console app] -
same domain, same rules, exposed over HTTP instead of a terminal.

## How to run

```
dotnet run --project TicketFlow.Api
```

Swagger's OpenAPI document is served at `/openapi/v1.json` in Development
(`app.MapOpenApi()`); there's no Swagger UI wired up yet, just the spec.

**Heads up:** storage is in-memory (`InMemoryTicketRepository`), so all tickets are lost
when the process restarts. That's intentional for now - see [How it's put together](#how-its-put-together).

## Endpoints

| Method | Route | Purpose | Success | Failure |
|---|---|---|---|---|
| `POST` | `/tickets` | Create a ticket | `201 Created` + `Location` header | `400` bad title/priority |
| `GET` | `/tickets?status=&priority=&assignee=` | List tickets, with optional filters | `200 OK` | `400` bad status/priority filter |
| `GET` | `/tickets/{id}` | Get one ticket | `200 OK` | `404` no such id |
| `PUT` | `/tickets/{id}` | Replace title / description / priority | `204 No Content` | `400` bad input, `404` no such id |
| `PATCH` | `/tickets/{id}/status` | Change status | `200 OK` + ticket | `400` bad status, `404` no such id |
| `PATCH` | `/tickets/{id}/assignee` | Set the assignee | `200 OK` + ticket | `400` missing username, `404` no such id |
| `GET` | `/tickets/stats` | Counts by status & priority, oldest open ticket | `200 OK` | - |

`{id}` is the same simple integer id shown in every response's `id` field (see
[Ids](#ids-are-just-integers) below) - `/tickets/1`, not a GUID.

Every error body has the same shape:

```json
{ "error": "Invalid status 'Bogus'. Valid values: Open, InProgress, Resolved, Closed." }
```

## Try it

```
dotnet run --project TicketFlow.Api
```

```
curl -i -X POST http://localhost:5181/tickets \
  -H "Content-Type: application/json" \
  -d '{"title":"Login page throws 500","priority":"High","assignedTo":"alice"}'

# HTTP/1.1 201 Created
# Location: http://localhost:5181/tickets/1
# {"id":1,"title":"Login page throws 500","description":null,"priority":"High",
#  "status":"Open","assignedTo":"alice","createdAt":"...","updatedAt":"...","shortId":"1"}

curl http://localhost:5181/tickets

curl -X PATCH http://localhost:5181/tickets/1/status \
  -H "Content-Type: application/json" -d '{"status":"InProgress"}'

curl http://localhost:5181/tickets/stats
```

## Ids are just integers

Tickets are numbered `1, 2, 3, ...` in the order they're created - assigned by whichever
`ITicketRepository` is in use, not chosen by the client. `POST` ignores any `id` you send
and returns the one it assigned; use that value (or the `Location` header) for every
later request.

## How it's put together

```
Controllers/  TicketsController - translates HTTP <-> TicketService calls, nothing else
Records/      Request DTOs (CreateTicketRequest, UpdateTicketRequest, ...)
Program.cs    DI registration (ITicketRepository, TicketService), middleware pipeline
```

None of the ticket domain or business logic lives in this project - it's all reused,
unchanged, from the same libraries the console app depends on:

* **`TicketFlow.Core`** - `Ticket`, `TicketPriority`, `TicketStatus`, `TicketService`
  (validation, filtering, stats), and the `ITicketRepository` abstraction.
* **`TicketFlow.Infrastructure`** - concrete repositories. This API is wired to
  `InMemoryTicketRepository`; the console app uses `JsonFileTicketRepository` instead.
  Swapping which one this API uses is a one-line change in `Program.cs`, not a rewrite.

`TicketsController` stays intentionally thin: every action either returns the result of
a `TicketService` call directly, or maps one of its exceptions to a status code
(`ArgumentException` -> 400, `InvalidOperationException` -> 404,
`TicketStoreException` -> 500). No validation or state-mutation logic lives in the
controller itself.



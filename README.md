# TicketFlow (CLI)

A support-ticket tracker you run from the console. Create tickets, filter them, change
their status, assign them to people, and check overall stats.

## Try it

```
dotnet run --project TicketFlow
```

With no arguments, TicketFlow drops you into an interactive session so you can try
several commands in a row:

```
ticketflow> add --title "Login page throws 500" --priority High --assignee alice
  [OK] Created ticket 1 - "Login page throws 500"

ticketflow> list
  ╔════╦════════════════════════╦══════════╦════════╦══════════╗
  ║ ID ║ TITLE                  ║ PRIORITY ║ STATUS ║ ASSIGNEE ║
  ╠════╬════════════════════════╬══════════╬════════╬══════════╣
  ║ 1  ║ Login page throws 500  ║ High     ║ Open   ║ alice    ║
  ╚════╩════════════════════════╩══════════╩════════╩══════════╝

ticketflow> status 1 InProgress
  [OK] Ticket 1 status set to InProgress.

ticketflow> exit
```

You can also run one command at a time from a regular shell:

```
dotnet run --project TicketFlow -- add --title "Login page throws 500" --priority High
dotnet run --project TicketFlow -- stats
```

**Heads up:** there's no save file yet, so data only lives for as long as one run.
Do all your testing in a single interactive session (as above) rather than across
separate `dotnet run` calls, or you'll just see an empty list each time.

## Screenshots

Startup banner and `help`:

![Banner and help screen](docs/screenshots/banner-and-help.png)

`add` (with a missing-title error first), `list`, and `search`:

![add, list, and search](docs/screenshots/add-list-search.png)

`stats --json`:

![stats as JSON](docs/screenshots/stats-json.png)

`stats` as a colored table, then `exit`:

![stats table and exit](docs/screenshots/stats-table.png)

## Commands

| Command | What it does |
|---|---|
| `add --title "..." [--description "..."] [--priority Low\|Medium\|High\|Critical] [--assignee name]` | Create a ticket. Priority defaults to Medium; status always starts Open. |
| `list [--status ...] [--priority ...] [--assignee ...] [--json]` | Show tickets, newest first. Any combination of filters can be used together. |
| `status <id> <NewStatus>` | Move a ticket to Open / InProgress / Resolved / Closed. |
| `assign <id> <username>` | Set or reassign who's working on it. |
| `search <text>` | Find tickets whose title or description mentions the text. |
| `stats [--json]` | Counts per status, counts per priority, and the oldest ticket still Open. |
| `help` | Print this command list from inside the app. |
| `exit` | Leave the interactive session. |

`<id>` is the simple numeric id shown by `list`/`add` - no GUIDs to copy-paste.

## How it's put together

```
Models/        Ticket (a record), TicketPriority, TicketStatus
Repositories/  ITicketRepository + an in-memory implementation
Services/      TicketService - the actual add/list/filter/stats logic
Cli/           turns typed input into a command, runs it, prints the result
```

The layering is deliberate: `TicketService` only knows about `ITicketRepository` (an
interface), never the concrete in-memory store - so swapping in real file or database
storage later is a new class, not a rewrite. Same idea with the console output: nothing
in `TicketService` prints or colors anything, so the logic stays testable without a
terminal attached.

`Ticket` is a record, and every change (status, assignee) produces a new copy via a
`with` expression rather than editing the ticket in place.

## Not built yet, on purpose

Saving to a file (so data survives a restart) and structured async error handling are
later milestones, not oversights - this stage focuses on the domain model, the
repository pattern, and LINQ-based filtering/stats.

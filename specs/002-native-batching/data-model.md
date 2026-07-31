# Data Model: Native Serilog Batching

This feature adds no persisted business data. Its model is the lifecycle of in-memory log-event batches, configuration, delivery outcomes, and failure notifications.

## Entity: Batching Configuration

| Field | Type | Default | Validation | Meaning |
|---|---|---|---|---|
| Batch size limit | Positive integer | 50 | Greater than zero | Maximum events sent in one bulk request. |
| Buffering time limit | Time span | 2 seconds | Greater than zero | Maximum normal delay before a partially filled batch is sent. |
| Eager first event | Boolean | `true` | Fixed for compatibility | Sends the first queued event without waiting for a full batch or full period. |
| Queue size limit | Integer or unbounded sentinel | 100,000 | `-1` or greater than zero | `-1` maps to an unbounded native queue; positive values bound memory use. |
| Retry time limit | Nullable time span | Native 10 minutes | Null, zero, or positive | Null retains the SDK default; zero exhausts on the first failure; positive values bound retry duration. |

**Relationship**: One non-durable sink registration owns one native batching configuration and one in-memory queue. Durable registrations do not consume this retry configuration.

## Entity: Log Event

An immutable Serilog event accepted by the logging pipeline.

| Attribute | Meaning |
|---|---|
| Identity | Object instance supplied by Serilog; no new persistent identifier is introduced. |
| Ordering | Queue order is preserved within the native batching lifecycle. |
| Delivery membership | An event belongs to one current batch at a time; after a completed partial response, accepted items are never requeued by this sink. |
| Item rejection | A rejected item in a valid completed response is terminal and routed through configured legacy item handlers. |

## Entity: Batch

An ordered, read-only collection of log events delivered in one OpenSearch bulk request.

| Attribute | Meaning |
|---|---|
| Size | One through the configured batch size limit. |
| Attempt count | Owned by native Serilog scheduling; the same batch remains current across failed attempts. |
| Retry deadline | Derived from the configured/native retry time limit. |
| Outcome | Valid completed response, invalid whole-batch outcome, or shutdown loss. |

## Entity: Delivery Outcome

| Outcome | Condition | Sink action | Native action |
|---|---|---|---|
| Completed success | Valid response with no rejected items | Return successfully | Clear batch and reset failure state. |
| Completed partial rejection | Valid response containing rejected items | Route only rejected items through legacy handlers, then return successfully | Clear batch; do not resend accepted items. |
| Whole-batch failure with exception | Request cannot complete or response supplies an original exception | Propagate exception | Report temporary and retry until exhausted. |
| Whole-batch failure without exception | Response is unsuccessful without an original exception | Throw a safe synthesized diagnostic exception | Report temporary and retry until exhausted. |
| Queue overflow | Native bounded queue cannot accept an event | No sink interception is available | Serilog 4.4 silently discards the event. |

## Entity: Failure Notification

| Kind | Trigger | Events | Fallback behavior |
|---|---|---|---|
| Temporary | A batch attempt throws | Current batch | Listener notified; fallback chain does not forward. |
| Permanent | Retry exhaustion drops a batch/backlog, or current shutdown batch fails | Affected batch/events | Listener notified; fallback receives affected events. |
| Final | Shutdown failure abandons remaining queue or native loop cannot finish | Remaining affected events, if known | Listener notified; fallback receives affected events. |

## State Transitions

```text
Accepted → Queued → Current Batch → Attempting
                                  ├─ valid complete response → Delivered
                                  ├─ valid partial response → Accepted items Delivered
                                  │                          └─ Rejected items Legacy Item Handling
                                  └─ exception/invalid response → Temporary Failure
                                                                 ├─ retry window remains → Current Batch
                                                                 └─ retry exhausted → Permanent Loss

Disposal → Flush Queued Events
         ├─ successful attempts → Delivered
         └─ first failed attempt → Current Batch Permanent Loss
                                 └─ Remaining Queue Final Loss
```

Any successful batch resets native failure scheduling before subsequent batches. Durable file-buffered events follow their existing independent state machine and are not represented by the non-durable queue above.

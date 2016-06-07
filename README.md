# Processing Pipeline example

This is a very basic example to implement a computational pipeline.

One of possible applications of such approach is as follows. Let's assume we need to implement a real-time computer vision system where a set of algorithms are applied consequently to images taken from camera. The main problem that may occur here is that some of the algorithms are slower than the frame rate, thus we do not have other choice than drop images that arrive faster than the algorithm is done working on a previous image. 

Pipeline features:

* Nodes start processing messages as soon as possible.
* A busy node stores the most recent message and process it once ready. If a new message arrives before the node finishes working on its task, the node stores the new message instead of the older one.
* Robust to corrupted messages.

Based on MailboxProcessor of F#. The better way is to use actors (see AKKA, Orleans and etc.).

Current implementation does not allow branching of the pipeline, each node imitates work by simple delay. Nodes have to be created in reverse order in order to configure output connections of nodes. After all nodes are created, run `Init` method for each of them.

## Example output

Let us assume delay between device readings is 300ms, there are three nodes with delays 100ms, 500ms, 200ms. The device sends  5 messages, and in the example below the third message is corrupted.

```
[1465290119514.00] Program starts
[1465290119515.50] Sending message = Task id:<0>
[1465290119515.50] mailbox {00}:        >HereIsTaskForYou: send task to worker: Task id:<0>
[1465290119516.00] worker {00}:         starts working on task {Task id:<0>}
[1465290119714.32] worker {00}:         finished working on task {Task id:<0>}
[1465290119714.82] mailbox {01}:        >HereIsTaskForYou: send task to worker: Task id:<0>
[1465290119714.82] worker {01}:         starts working on task {Task id:<0>}
[1465290119714.82] mailbox {00}:        >ReadyToWork: task is not set yet
[1465290119809.93] Sending message = Task id:<1>
[1465290119809.93] mailbox {00}:        >HereIsTaskForYou: send task to worker: Task id:<1>
[1465290119810.42] worker {00}:         starts working on task {Task id:<1>}
[1465290120010.19] worker {00}:         finished working on task {Task id:<1>}
[1465290120010.19] mailbox {01}:        >HereIsTaskForYou: worker is busy, dropping message
[1465290120010.69] mailbox {00}:        >ReadyToWork: task is not set yet
[1465290120110.12] Input message is corrupted
[1465290120110.12] mailbox {00}:        >HereIsTaskForYou: task is not set yet
[1465290120223.27] worker {01}:         finished working on task {Task id:<0>}
[1465290120223.27] mailbox {02}:        >HereIsTaskForYou: send task to worker: Task id:<0>
[1465290120223.27] mailbox {01}:        >ReadyToWork: end task to worker: Task id:<1>
[1465290120223.27] worker {02}:         starts working on task {Task id:<0>}
[1465290120223.27] worker {01}:         starts working on task {Task id:<1>}
[1465290120331.69] worker {02}:         finished working on task {Task id:<0>}
[1465290120332.18] worker {02}:         next actor is not set
[1465290120332.18] mailbox {02}:        >ReadyToWork: task is not set yet
[1465290120410.20] Sending message = Task id:<3>
[1465290120410.70] mailbox {00}:        >HereIsTaskForYou: send task to worker: Task id:<3>
[1465290120410.70] worker {00}:         starts working on task {Task id:<3>}
[1465290120622.86] worker {00}:         finished working on task {Task id:<3>}
[1465290120622.86] mailbox {01}:        >HereIsTaskForYou: worker is busy, dropping message
[1465290120623.36] mailbox {00}:        >ReadyToWork: task is not set yet
[1465290120710.68] Sending message = Task id:<4>
[1465290120710.68] mailbox {00}:        >HereIsTaskForYou: send task to worker: Task id:<4>
[1465290120711.19] worker {00}:         starts working on task {Task id:<4>}
[1465290120724.47] worker {01}:         finished working on task {Task id:<1>}
[1465290120724.47] mailbox {02}:        >HereIsTaskForYou: send task to worker: Task id:<1>
[1465290120724.98] mailbox {01}:        >ReadyToWork: end task to worker: Task id:<3>
[1465290120725.47] worker {02}:         starts working on task {Task id:<1>}
[1465290120725.97] worker {01}:         starts working on task {Task id:<3>}
[1465290120832.50] worker {02}:         finished working on task {Task id:<1>}
[1465290120832.50] worker {02}:         next actor is not set
[1465290120833.00] mailbox {02}:        >ReadyToWork: task is not set yet
[1465290120910.72] worker {00}:         finished working on task {Task id:<4>}
[1465290120911.22] mailbox {01}:        >HereIsTaskForYou: worker is busy, dropping message
[1465290120911.71] mailbox {00}:        >ReadyToWork: task is not set yet
[1465290121010.98] Sending message = Task id:<5>
[1465290121010.98] mailbox {00}:        >HereIsTaskForYou: send task to worker: Task id:<5>
[1465290121011.48] worker {00}:         starts working on task {Task id:<5>}
[1465290121211.32] worker {00}:         finished working on task {Task id:<5>}
[1465290121211.32] mailbox {01}:        >HereIsTaskForYou: worker is busy, dropping message
[1465290121212.30] mailbox {00}:        >ReadyToWork: task is not set yet
[1465290121224.48] worker {01}:         finished working on task {Task id:<3>}
[1465290121224.48] mailbox {01}:        >ReadyToWork: end task to worker: Task id:<5>
[1465290121225.98] worker {01}:         starts working on task {Task id:<5>}
[1465290121225.98] mailbox {02}:        >HereIsTaskForYou: send task to worker: Task id:<3>
[1465290121226.48] worker {02}:         starts working on task {Task id:<3>}
[1465290121332.24] worker {02}:         finished working on task {Task id:<3>}
[1465290121332.73] worker {02}:         next actor is not set
[1465290121332.73] mailbox {02}:        >ReadyToWork: task is not set yet
[1465290121738.83] worker {01}:         finished working on task {Task id:<5>}
[1465290121738.83] mailbox {02}:        >HereIsTaskForYou: send task to worker: Task id:<5>
[1465290121739.33] mailbox {01}:        >ReadyToWork: task is not set yet
[1465290121740.83] worker {02}:         starts working on task {Task id:<5>}
[1465290121851.80] worker {02}:         finished working on task {Task id:<5>}
[1465290121851.80] worker {02}:         next actor is not set
[1465290121852.79] mailbox {02}:        >ReadyToWork: task is not set yet
[1465290123311.76] Program finished. Press ENTER to quit ...
```

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

Let us assume delay between device readings is 300ms, there are three nodes with delays 100ms, 500ms, 200ms. The device sends  5 messages, and in the example below the second message is corrupted.

```
[1465332960.8799] Program starts
[1465332960.8854] Nodes:                 @input=`00`, @internal=`01`, @output=`02`
[1465332960.8859] Delays, ms: @loop=300, @input=200,  @internal=500,  @output=100
[1465332960.8864] Loop starts, 5 messages will be sent, with 10% chance of message corruption
[1465332960.8869]       (!) Sending message  ``Task id:<0>''
[1465332960.8869] mailbox {00}: >> HereIsTaskForYou: send task to worker: Task id:<0>
[1465332960.8874] worker  (00): => starts working on task {Task id:<0>}
[1465332961.0441] worker  (00): <= finished working on task {Task id:<0>}
[1465332961.0446] mailbox {01}: >> HereIsTaskForYou: send task to worker: Task id:<0>
[1465332961.0451] worker  (01): => starts working on task {Task id:<0>}
[1465332961.0456] mailbox {00}: >> ReadyToWork: task is not set (yet)
[1465332961.1140]       (!) Input message is corrupted
[1465332961.1140] mailbox {00}: >> HereIsTaskForYou: task is not set (yet)
[1465332961.4149]       (!) Sending message  ``Task id:<2>''
[1465332961.4154] mailbox {00}: >> HereIsTaskForYou: send task to worker: Task id:<2>
[1465332961.4159] worker  (00): => starts working on task {Task id:<2>}
[1465332961.5450] worker  (01): <= finished working on task {Task id:<0>}
[1465332961.5455] mailbox {02}: >> HereIsTaskForYou: send task to worker: Task id:<0>
[1465332961.5460] mailbox {01}: >> ReadyToWork: task is not set (yet)
[1465332961.5465] worker  (02): => starts working on task {Task id:<0>}
[1465332961.6155] worker  (00): <= finished working on task {Task id:<2>}
[1465332961.6160] mailbox {00}: >> ReadyToWork: task is not set (yet)
[1465332961.6165] mailbox {01}: >> HereIsTaskForYou: send task to worker: Task id:<2>
[1465332961.6165] worker  (01): => starts working on task {Task id:<2>}
[1465332961.6460] worker  (02): <= finished working on task {Task id:<0>}
[1465332961.6465] worker  (02): >> HereIsTaskForYou: next actor is not set
[1465332961.6470] mailbox {02}: >> ReadyToWork: task is not set (yet)
[1465332961.7154]       (!) Sending message  ``Task id:<3>''
[1465332961.7154] mailbox {00}: >> HereIsTaskForYou: send task to worker: Task id:<3>
[1465332961.7159] worker  (00): => starts working on task {Task id:<3>}
[1465332961.9158] worker  (00): <= finished working on task {Task id:<3>}
[1465332961.9163] mailbox {01}: >> HereIsTaskForYou: worker is busy, dropping message
[1465332961.9168] mailbox {00}: >> ReadyToWork: task is not set (yet)
[1465332962.0158]       (!) Sending message  ``Task id:<4>''
[1465332962.0158] mailbox {00}: >> HereIsTaskForYou: send task to worker: Task id:<4>
[1465332962.0168] worker  (00): => starts working on task {Task id:<4>}
[1465332962.1157] worker  (01): <= finished working on task {Task id:<2>}
[1465332962.1162] mailbox {02}: >> HereIsTaskForYou: send task to worker: Task id:<2>
[1465332962.1167] mailbox {01}: >> ReadyToWork: end task to worker: Task id:<3>
[1465332962.1177] worker  (02): => starts working on task {Task id:<2>}
[1465332962.1182] worker  (01): => starts working on task {Task id:<3>}
[1465332962.2286] worker  (02): <= finished working on task {Task id:<2>}
[1465332962.2291] worker  (02): >> HereIsTaskForYou: next actor is not set
[1465332962.2296] worker  (00): <= finished working on task {Task id:<4>}
[1465332962.2306] mailbox {01}: >> HereIsTaskForYou: worker is busy, dropping message
[1465332962.2316] mailbox {00}: >> ReadyToWork: task is not set (yet)
[1465332962.2321] mailbox {02}: >> ReadyToWork: task is not set (yet)
[1465332962.3161]       (!) Sending message  ``Task id:<5>''
[1465332962.3166] mailbox {00}: >> HereIsTaskForYou: send task to worker: Task id:<5>
[1465332962.3171] worker  (00): => starts working on task {Task id:<5>}
[1465332962.5164] worker  (00): <= finished working on task {Task id:<5>}
[1465332962.5169] mailbox {01}: >> HereIsTaskForYou: worker is busy, dropping message
[1465332962.5179] mailbox {00}: >> ReadyToWork: task is not set (yet)
[1465332962.6165] worker  (01): <= finished working on task {Task id:<3>}
[1465332962.6170] mailbox {01}: >> ReadyToWork: end task to worker: Task id:<5>
[1465332962.6180] mailbox {02}: >> HereIsTaskForYou: send task to worker: Task id:<3>
[1465332962.6190] worker  (01): => starts working on task {Task id:<5>}
[1465332962.6200] worker  (02): => starts working on task {Task id:<3>}
[1465332962.7289] worker  (02): <= finished working on task {Task id:<3>}
[1465332962.7294] worker  (02): >> HereIsTaskForYou: next actor is not set
[1465332962.7299] mailbox {02}: >> ReadyToWork: task is not set (yet)
[1465332963.1163] worker  (01): <= finished working on task {Task id:<5>}
[1465332963.1168] mailbox {02}: >> HereIsTaskForYou: send task to worker: Task id:<5>
[1465332963.1173] mailbox {01}: >> ReadyToWork: task is not set (yet)
[1465332963.1183] worker  (02): => starts working on task {Task id:<5>}
[1465332963.2233] worker  (02): <= finished working on task {Task id:<5>}
[1465332963.2238] worker  (02): >> HereIsTaskForYou: next actor is not set
[1465332963.2248] mailbox {02}: >> ReadyToWork: task is not set (yet)
[1465332964.6166] Program finished. Press ENTER to quit ...
```

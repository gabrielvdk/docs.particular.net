---
title: Azure Service Long Running Operations
reviewed: 2016-09-06
tags:
- Azure
- Cloud
- Error Handling
---

When using messaging, long running operations are discouraged. This is no exception when running in a cloud environment either. Azure Service Bus service takes a step further in enforcing this practice by having a period of time within which message processing should be done. Whenever a message is picked by one of the competing consumers for processing, the message is locked for a certain period. In case of processing time exceeding the allotted period of time, ASB service unlocks the message to other competing consumer to try to process the message again. This period of time is defined as [`LockDuration`](/nservicebus/azure-service-bus/configuration/full.md). ASB allows setting `LockDuration` up to the maximum of 5 minutes.

What will happen to a handler that takes longer than 5 minutes to complete its work? `LockDuration` will expire, the message will re-appear and will be processed again. If all every attempt results in long processing time that exceeds the maximum time allowed, the message will surpass the `MaxDeliveryCount` specified. Once `MaxDeliveryCount` is reached, the message will be dead-lettered and moved to the appropriate dead letter queue.


## How to handled long running operations

There are several options to handle long running operations.


### Reducing processing time

Review handler implementation and determine if work can be split into several handlers. This could mean splitting message processing handler into multiple messages and handlers to handle those. E.g. handling a batch of records as a single message. Instead, a batch of records could be split into multiple individual messages, with a message per record, and handled in a separate handler. This would reduce overall processing from a total of all records to a single record processing time, ensuring it's well under maximum `LockDuration` time.

--V7 partial--
### Using message lock renewal feature

Versions 7 and above of the transport, there's an option to extend `LockDuration` time by using [message lock renewal](/nservicebus/azure-service-bus/configuration/full.md#controlling-connectivity-message-receivers) feature. See [`AutoRenewTimeout`](/nservicebus/azure-service-bus/message-lock-renewal.md#configuring-message-lock-renewal) for details on how to configure message lock renewal.

Note: When message lock renewal is used, it will be applied to all entities. It means all messages would get the same extended processing time set by `AutoRenewTimeout` in case processing lasts longer than `LockDuration` time. While it can be perceived as a valuable option, it also can trigger longer processing times for messages that should not necessarily take long to process, masquerading problem in the code. 


### Performing processing outside of a message handler

Explanation
V*: external process or a background thread theory + sample
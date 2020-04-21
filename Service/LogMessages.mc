;// **********
;// Categories
;// **********

MessageId=0x1
Severity=Success
SymbolicName=CATEGORY_APPLICATION_ERROR
Language=English
Application Error
.

MessageId=0x2
SymbolicName=CATEGORY_MODULE_ERROR
Module Error
.

MessageId=0x3
SymbolicName=CATEGORY_DEBUG_MESSAGE
Debug
.

MessageId=0x3
SymbolicName=CATEGORY_MAGIC_PACKET
Magic Packet
.

MessageId=0x4
SymbolicName=CATEGORY_CHECKPOINT
Checkpoint
.

;// ********
;// Messages
;// ********

;// 1: Error message
;// 2: Error type
MessageId=1000
SymbolicName=APPLICATION_HALT_ERROR
Halting due to unexpected error.
%%97007 %1
%%97008 %2
.

;// 1: Module name
;// 2: Error message
;// 3: Error type
MessageId=1001
SymbolicName=MODULE_ERROR
An unexpected error occurred.
Module: %1
%%97007 %2
%%97008 %3
.

;// 1: Registry path
;// 2: Error message
;// 3: Error type
MessageId=1011
SymbolicName=REGISTRY_ACCESS_ERROR
%%97009 %%96010
%%97007 %2
%%97008 %3
.

;// 1: Registry path
;// 2: Error message
;// 3: Error type
MessageId=1012
SymbolicName=REGISTRY_OPENKEY_ERROR
%%97009 %%96011
%%97007 %2
%%97008 %3
.

;// 1: VNIC Instance ID
MessageId=1021
SymbolicName=INVALID_VIRTUAL_ADAPTER
Invalid virtual network adapter instance ID %1.
.

;// No special errors at this time. Rolled up into 1001
;// MessageId=1022
;// SymbolicName=VIRTUAL_ADAPTER_SUBSCRIBER_ERROR
;// Virtual adapter subscriber received an error.
;// Type: %1
;// Message: %2
;// .

;// 1: Target MAC address
;// 2: Requesting IP address
MessageId=2000
SymbolicName=MAGIC_PACKET_PROCESSED
Received wake-on-LAN frame for MAC address %1 from IP address %2.
.

;// 1: VM name
;// 2: VM ID
;// 3: VM MAC address
;// 4: WOL request source IP
MessageId=3000
SymbolicName=VIRTUAL_MACHINE_START_SUCCESS
%99900 %%96006.
%%97000 %1
%%97001 %2
%%96000 %%97002 %3
%%97003 96001: %4
.

;// 1: VM name
;// 2: VM ID
;// 3: VM MAC address
;// 4: WOL request source IP
;// 5: Result code
;// 6: Result message
MessageId=3001
SymbolicName=VIRTUAL_MACHINE_START_FAIL
%99900 %%96007.
%%97000 %1
%%97001 %2
%%96000 %%97002 %3
%%97003 96001: %4
%%97004: %5
%%97005: %6
.

;// 1: Checkpoint action
;// 2: VM name
;// 3: User name that started the job
;// 4: VM ID
;// 5: Job ID
MessageId=4000
SymbolicName=CHECKPOINT_ACTION_STARTED
%%96004 "%1" %%96005
%%96000: %2
%%97006 %3
%%96000 %%97001 %4
%%96003 %%97001 %5
.

;// 1: Checkpoint action
;// 2: VM name
;// 3: User name that started the job
;// 4: VM ID
;// 5: Job ID
MessageId=4001
SymbolicName=CHECKPOINT_ACTION_SUCCESS
%%96004 "%1" %%96006
%%96000: %2
%%97006 %3
%%96000 %%97001 %4
%%96003 %%97001 %5
.

;// 1: Checkpoint action
;// 2: VM name
;// 3: User name that started the job
;// 4: VM ID
;// 5: Job ID
;// 6: Job error/result code
;// 7: Job error/result message
MessageId=4002
SymbolicName=CHECKPOINT_ACTION_FAIL
%%96004 "%1" %%96007
%%96000: %2
%%97006 %3
%%96000 %%97001 %4
%%96003 %%97001 %5
%%97004 %6
%%97005 %7
.

;// 1: Message as input
MessageId=9000
SymbolicName=DEBUG_MESSAGE_GENERIC
%1
.

;// 1: Debug mode switch setting
MessageId=9001
SymbolicName=DEBUG_DEBUG_MODE_CHANGED
Debug mode set to %1
.

;// 1: KVP name
;// 2: Registry path
MessageId=9002
SymbolicName=DEBUG_REGISTRY_KVP_NOTFOUND
Registry KVP not found.
KVP name: %1
%%97010 %2
.

;// 1: Number of virtual adapters
MessageId=9003
SymbolicName=DEBUG_VIRTUAL_ADAPTER_ENUMERATED_COUNT
Enumerated %1 virtual adapters.
.

;// 1: MAC address
;// 2: emulated or synthetic
MessageId=9004
SymbolicName=DEBUG_VIRTUAL_ADAPTER_NEW
%%96012 %%96013
%%97002 %1
%%97008 %2
.

;// 1: MAC address
;// 2: emulated or synthetic
MessageId=9005
SymbolicName=DEBUG_VIRTUAL_ADAPTER_CHANGED
%%96012 %%96014
%%97002 %1
%%97008 %2
.

;// 1: MAC address
;// 2: emulated or synthetic
MessageId=9006
SymbolicName=DEBUG_VIRTUAL_ADAPTER_NEW_FROM_UPDATE
%%96012 %%96013 from update event
%%97002 %1
%%97008 %2
.

;// 1: MAC address
;// 2: emulated or synthetic
MessageId=9007
SymbolicName=DEBUG_VIRTUAL_ADAPTER_DELETED
%%96012 %%96015
%%97002 %1
%%97008 %2
.

MessageId=9008
SymbolicName=DEBUG_INITIATED_VM_START
Created start job with instance ID %1 for VM %2 with ID %3
.

MessageId=95000
SymbolicName=MODULENAME_REGISTRY
Registry
.

MessageId=95001
SymbolicName=MODULENAME_WAKEONLAN
Wake-on-LAN
.

MessageId=96000
SymbolicName=TEMPLATE_COMPONENT_VIRTUAL_MACHINE
Virtual Machine%0
.

MessageId=96001
SymbolicName=TEMPLATE_COMPONENT_IP_ADDRESS
IP Address%0
.

MessageId=96002
SymbolicName=TEMPLATE_COMPONENT_ID
ID%0
.

MessageId=96003
SymbolicName=TEMPLATE_COMPONENT_JOB
Job%
.

MessageId=96004
SymbolicName=TEMPLATE_COMPONENT_CHECKPOINT_ACTION
Checkpoint action%0
.

MessageId=96005
SymbolicName=TEMPLATE_COMPONENT_STARTED
started%0
.

MessageId=96006
SymbolicName=TEMPLATE_COMPONENT_SUCCEEDED
succeeded%0
.

MessageId=96007
SymbolicName=TEMPLATE_COMPONENT_FAILED
failed%0
.

MessageId=96008
SymbolicName=TEMPLATE_COMPONENT_ERROR
Error%0
.

MessageId=96009
SymbolicName=TEMPLATE_COMPONENT_RESULT
Result%0
.

MessageId=96010
SymbolicName=TEMPLATE_COMPONENT_READ
Read%0
.

MessageId=96011
SymbolicName=TEMPLATE_COMPONENT_OPEN
Open%0
.

MessageId=96012
SymbolicName=TEMPLATE_COMPONENT_VIRTUAL_ADAPTER
Virtual network adapter%0
.

MessageId=96013
SymbolicName=TEMPLATE_COMPONENT_CREATED
created%0
.

MessageId=96014
SymbolicName=TEMPLATE_COMPONENT_CHANGED
changed%0
.

MessageId=96015
SymbolicName=TEMPLATE_COMPONENT_DELETED
deleted%0
.

MessageId=96016
SymbolicName=TEMPLATE_COMPONENT_EMULATED
emulated%0
.

MessageId=96017
SymbolicName=TEMPLATE_COMPONENT_SYNTHETIC
synthetic%0
.

MessageId=96018
SymbolicName=TEMPLATE_COMPONENT_TYPE
type%0

MessageId=97000
SymbolicName=SUBTEMPLATE_VIRTUAL_MACHINE_NAME
%%96000 name:%0
.

MessageId=97001
SymbolicName=SUBTEMPLATE_VIRTUAL_MACHINE_ID
%%96000 ID:%0
.

MessageId=97002
SymbolicName=SUBTEMPLATE_MAC_ADDRESS
MAC address:%0
.

MessageId=97003
SymbolicName=SUBTEMPLATE_REQUEST_SOURCE
Request source%0
.

MessageId=97004
SymbolicName=SUBTEMPLATE_RESULT_CODE
%%96009 code:%0
.

MessageId=97005
SymbolicName=SUBTEMPLATE_RESULT_MESSAGE
%%96009 message:%0
.

MessageId=97006
SymbolicName=SUBTEMPLATE_INITIATOR
Initiator:%0
.

MessageId=97007
SymbolicName=SUBTEMPLATE_ERROR_MESSAGE
%%96008 message:%0
.

MessageId=97008
SymbolicName=SUBTEMPLATE_ERROR_TYPE
%%96008 %%96018:%0
.

MessageId=97009
SymbolicName=SUBTEMPLATE_REGISTRY_ACCESS
Registry access error.
Action:%0
.

MessageId=97010
SymbolicName=SUBTEMPLATE_PATH
Path:%0
.

MessageId=97008
SymbolicName=SUBTEMPLATE_VIRTUAL_ADAPTER_TYPE
%%96012 %%96018:%0

MessageId=99900
SymbolicName=WAKEON_LAN_OP_TEMPLATE
%%99000 %%95001 start operation
.

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

MessageId=1000
SymbolicName=APPLICATION_HALT_ERROR
Halting due to unexpected error %1 of type %2.
.

MessageId=1001
SymbolicName=MODULE_ERROR
Unexpected error of type %1 in module %2: %3.
%1 module encountered an error.
Type: %2
Message: %3
.

MessageId=1011
SymbolicName=REGISTRY_ACCESS_ERROR
An error occurred while accessing the registry at %1: %2.
.

MessageId=1012
SymbolicName=REGISTRY_KEY_ERROR
An error occurred while opening registry key %1: %2
.

MessageId=1021
SymbolicName=INVALID_VIRTUAL_ADAPTER
Invalid virtual network adapter instance ID %1.
.

MessageId=1022
SymbolicName=VIRTUAL_ADAPTER_SUBSCRIBER_ERROR
Virtual adapter subscriber received an error.
Type: %1
Message: %2
.

MessageId=2000
SymbolicName=MAGIC_PACKET_PROCESSED
Received wake-on-LAN frame for MAC address %1 from IP address %2.
.

MessageId=3000
SymbolicName=VIRTUAL_MACHINE_START_SUCCESS
%99900 %%96006.
%%97000 %1
%%97001 %2
%%96000 %%97002 %3
%%97003 96001: %4
%%97004: %5
%%97005: %6
.

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

MessageId=4000
SymbolicName=CHECKPOINT_ACTION_STARTED
%%96004 "%1" %%96005
%%96000: %2
%%97006 %3
%%96000 %%97001 %4
%%96003 %%97001 %5
.

MessageId=4001
SymbolicName=CHECKPOINT_ACTION_SUCCESS
%%96004 "%1" %%96006
%%96000: %2
%%97006 %3
%%96000 %%97001 %4
%%96003 %%97001 %5
.

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

MessageID=96007
SymbolicName=TEMPLATE_COMPONENT_FAILED
failed%0
.

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
Result code:%0
.

MessageId=97005
SymbolicName=SUBTEMPLATE_RESULT_MESSAGE
Result message:%0
.

MessageID=97006
SymbolicName=SUBTEMPLATE_INITIATOR
Initiator:%0

MessageId=99900
SymbolicName= WAKEON_LAN_OP_TEMPLATE
%%99000 %%95001 start operation
.
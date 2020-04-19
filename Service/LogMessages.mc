;// **********
;// Categories
;// **********

MessageId=0x1
Severity=Success
SymbolicName=APPLICATION_ERROR
Language=English
Application Error
.

MessageId=0x2
SymbolicName=MODULE_ERROR
Module Error
.

MessageId=0x3
SymbolicName=DEBUG_MESSAGE
Debug
.

MessageId=0x3
SymbolicName=MAGIC_PACKET
Magic Packet
.

MessageId=0x4
SymbolicName=CHECKPOINT
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

MessageId=1002
SymbolicName=MODULE_REGISTRY
Registry
.

MessageId=1003
SymbolicName=MODULE_WAKEONLAN
Wake On Lan
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
.
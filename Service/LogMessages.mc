MessageIdTypedef=WORD
LanguageNames=(English=0x409:MSG00409)
OutputBase=10

;// **********
;// Categories
;// **********

MessageId=1
Severity=Success
SymbolicName=CATEGORY_APPLICATION_ERROR
Language=English
Application Error
.

MessageId=2
SymbolicName=CATEGORY_MODULE_ERROR
Language=English
Module Error
.

MessageId=3
SymbolicName=CATEGORY_DEBUG_MESSAGE
Language=English
Debug
.

MessageId=4
SymbolicName=CATEGORY_MAGIC_PACKET
Language=English
Magic Packet
.

MessageId=5
SymbolicName=CATEGORY_VM_STARTER
Language=English
Virtual Machine Starter
.

MessageId=6
SymbolicName=CATEGORY_CHECKPOINT
Language=English
Checkpoint
.

MessageId=7
SymbolicName=CATEGORY_CIM_ERROR
Language=English
CIM/WMI Error
.

;// ************
;// Module Names
;// ************

MessageId=500
SymbolicName=MODULENAME_REGISTRY
Language=English
%%619
.

MessageId=501
SymbolicName=MODULENAME_WAKEONLAN
Language=English
Wake-on-LAN
.

;// *******************
;// Template Components
;// *******************

MessageId=600
SymbolicName=TEMPLATE_COMPONENT_VIRTUAL_MACHINE
Language=English
Virtual Machine%0
.

MessageId=601
SymbolicName=TEMPLATE_COMPONENT_IP_ADDRESS
Language=English
IP Address%0
.

MessageId=602
SymbolicName=TEMPLATE_COMPONENT_ID
Language=English
ID%0
.

MessageId=603
SymbolicName=TEMPLATE_COMPONENT_JOB
Language=English
Job%
.

MessageId=604
SymbolicName=TEMPLATE_COMPONENT_CHECKPOINT_ACTION
Language=English
Checkpoint action%0
.

MessageId=605
SymbolicName=TEMPLATE_COMPONENT_STARTED
Language=English
started%0
.

MessageId=606
SymbolicName=TEMPLATE_COMPONENT_SUCCEEDED
Language=English
succeeded%0
.

MessageId=607
SymbolicName=TEMPLATE_COMPONENT_FAILED
Language=English
failed%0
.

MessageId=608
SymbolicName=TEMPLATE_COMPONENT_ERROR
Language=English
Error%0
.

MessageId=609
SymbolicName=TEMPLATE_COMPONENT_RESULT
Language=English
Result%0
.

MessageId=610
SymbolicName=TEMPLATE_COMPONENT_READ
Language=English
Read%0
.

MessageId=611
SymbolicName=TEMPLATE_COMPONENT_OPEN
Language=English
Open%0
.

MessageId=612
SymbolicName=TEMPLATE_COMPONENT_VIRTUAL_ADAPTER
Language=English
Virtual network adapter%0
.

MessageId=613
SymbolicName=TEMPLATE_COMPONENT_CREATED
Language=English
created%0
.

MessageId=614
SymbolicName=TEMPLATE_COMPONENT_CHANGED
Language=English
changed%0
.

MessageId=615
SymbolicName=TEMPLATE_COMPONENT_DELETED
Language=English
deleted%0
.

MessageId=616
SymbolicName=TEMPLATE_COMPONENT_EMULATED
Language=English
emulated%0
.

MessageId=617
SymbolicName=TEMPLATE_COMPONENT_SYNTHETIC
Language=English
synthetic%0
.

MessageId=618
SymbolicName=TEMPLATE_COMPONENT_TYPE
Language=English
type%0
.

MessageId=619
SymbolicName=TEMPLATE_COMPONENT_REGISTRY
Language=English
Registry%0
.

MessageId=620
SymbolicName=TEMPLATE_COMPONENT_KVP
Language=English
KVP%0
.

;// *************
;// Sub-templates
;// *************

MessageId=700
SymbolicName=SUBTEMPLATE_VIRTUAL_MACHINE_NAME
Language=English
%%600 name:%0
.

MessageId=701
SymbolicName=SUBTEMPLATE_VIRTUAL_MACHINE_ID
Language=English
%%600 ID:%0
.

MessageId=702
SymbolicName=SUBTEMPLATE_MAC_ADDRESS
Language=English
MAC address:%0
.

MessageId=703
SymbolicName=SUBTEMPLATE_REQUEST_SOURCE
Language=English
Request source%0
.

MessageId=704
SymbolicName=SUBTEMPLATE_RESULT_CODE
Language=English
%%609 code:%0
.

MessageId=705
SymbolicName=SUBTEMPLATE_RESULT_MESSAGE
Language=English
%%609 message:%0
.

MessageId=706
SymbolicName=SUBTEMPLATE_INITIATOR
Language=English
Initiator:%0
.

MessageId=707
SymbolicName=SUBTEMPLATE_ERROR_MESSAGE
Language=English
%%608 message:%0
.

MessageId=708
SymbolicName=SUBTEMPLATE_ERROR_TYPE
Language=English
%%608 %%618:%0
.

MessageId=709
SymbolicName=SUBTEMPLATE_REGISTRY_ACCESS
Language=English
%%619 access error.
Action:%0
.

MessageId=710
SymbolicName=SUBTEMPLATE_PATH
Language=English
Path:%0
.

MessageId=711
SymbolicName=SUBTEMPLATE_VIRTUAL_ADAPTER_TYPE
Language=English
%%612 %%618:%0
.

MessageId=712
SymbolicName=SUBTEMPLATE_REGISTRY_PATH
Language=English
%%619 %%710: %0
.

MessageId=713
SymbolicName=SUBTEMPLATE_VIRTUAL_MACHINE_BOOT_ATTEMPT
Language=English
%%600 boot-up attempt%0
.

MessageId=714
SymbolicName=SUBTEMPLATE_MODULE
Language=English
Module:%0
.

;// ********
;// Messages
;// ********

;// 1: Error message
;// 2: Error type
MessageId=1000
SymbolicName=APPLICATION_HALT_ERROR
Language=English
Halting due to unexpected error.
%%707 %1
%%708 %2
.

;// 1: Module name
;// 2: Error message
;// 3: Error type
MessageId=1001
SymbolicName=MODULE_ERROR
Language=English
An unexpected error occurred.
%%714 %1
%%707 %2
%%708 %3
.

;// 1: Error message
;// 2: Module name
MessageId=1002
SymbolicName=CIM_ERROR
Language=English
An error occurred in the CIM/WMI subsystem.
%%707 %1
%%714 %2
.

MessageId=1003
SymbolicName=ELEVATION_ERROR
Language=English
Must run as an elevated user.
.

;// 1: Registry path
;// 2: Error message
;// 3: Error type
MessageId=1011
SymbolicName=REGISTRY_ACCESS_ERROR
Language=English
%%709 %%610
%%712 %1
%%707 %2
%%708 %3
.

;// 1: Registry path
;// 2: Error message
;// 3: Error type
MessageId=1012
SymbolicName=REGISTRY_OPENKEY_ERROR
Language=English
%%709 %%611
%%712 %1
%%707 %2
%%708 %3
.

;// 1: VNIC Instance ID
MessageId=1021
SymbolicName=INVALID_VIRTUAL_ADAPTER
Language=English
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
Language=English
Received wake-on-LAN frame
%%702 %1
%%703 %%601: %2.
.

;// 1: VM name
;// 2: VM ID
;// 3: VM MAC address
;// 4: WOL request source IP
MessageId=3000
SymbolicName=VIRTUAL_MACHINE_START_SUCCESS
Language=English
%713 %%606.
%%700 %1
%%701 %2
%%600 %%702 %3
%%703 %%601: %4
.

;// 1: VM name
;// 2: VM ID
;// 3: VM MAC address
;// 4: WOL request source IP
;// 5: Result code
;// 6: Result message
MessageId=3001
SymbolicName=VIRTUAL_MACHINE_START_FAIL
Language=English
%713 %%607.
%%700 %1
%%701 %2
%%600 %%702 %3
%%703 %%601: %4
%%704: %5
%%705: %6
.

;// 1: Checkpoint action
;// 2: VM name
;// 3: User name that started the job
;// 4: VM ID
;// 5: Job ID
MessageId=4000
SymbolicName=CHECKPOINT_ACTION_STARTED
Language=English
%%604 "%1" %%605
%%600: %2
%%706 %3
%%600 %%701 %4
%%603 %%701 %5
.

;// 1: Checkpoint action
;// 2: VM name
;// 3: User name that started the job
;// 4: VM ID
;// 5: Job ID
MessageId=4001
SymbolicName=CHECKPOINT_ACTION_SUCCESS
Language=English
%%604 "%1" %%606
%%600: %2
%%706 %3
%%600 %%701 %4
%%603 %%701 %5
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
Language=English
%%604 "%1" %%607
%%600: %2
%%706 %3
%%600 %%701 %4
%%603 %%701 %5
%%704 %6
%%705 %7
.

;// 1: Message as input
;// 2: Module name
MessageId=9000
SymbolicName=DEBUG_MESSAGE_GENERIC
Language=English
%1
%%714 %2
.

;// 1: Debug mode switch setting
MessageId=9001
SymbolicName=DEBUG_DEBUG_MODE_CHANGED
Language=English
Debug mode set to %1
.

;// 1: KVP name
;// 2: Registry path
MessageId=9002
SymbolicName=DEBUG_REGISTRY_KVP_NOTFOUND
Language=English
%%619 %%620 not found.
%%620 name: %1
%%710 %2
.

;// 1: Number of virtual adapters
MessageId=9003
SymbolicName=DEBUG_VIRTUAL_ADAPTER_ENUMERATED_COUNT
Language=English
Enumerated %1 virtual network adapters.
.

;// 1: MAC address
;// 2: emulated or synthetic
MessageId=9004
SymbolicName=DEBUG_VIRTUAL_ADAPTER_NEW
Language=English
%%612 %%613
%%702 %1
%%711 %2
.

;// 1: MAC address
;// 2: emulated or synthetic
MessageId=9005
SymbolicName=DEBUG_VIRTUAL_ADAPTER_CHANGED
Language=English
%%612 %%614
%%702 %1
%%711 %2
.

;// 1: MAC address
;// 2: emulated or synthetic
MessageId=9006
SymbolicName=DEBUG_VIRTUAL_ADAPTER_NEW_FROM_UPDATE
Language=English
%%612 %%613 from update event
%%702 %1
%%711 %2
.

;// 1: MAC address
;// 2: emulated or synthetic
MessageId=9007
SymbolicName=DEBUG_VIRTUAL_ADAPTER_DELETED
Language=English
%%612 %%615
%%702 %1
%%711 %2
.

;// 1: VM name
;// 2: VM ID
;// 3: Job ID
MessageId=9008
SymbolicName=DEBUG_INITIATED_VM_START
Language=English
Initiated %%600 boot-up
%%600: %1
%%600 %%602: %2
%%603 %%602: %3
.

MessageId=9009
SymbolicName=DEBUG_MAGIC_PACKET_INVALID_FORMAT
Language=English
Possible magic packet received with invalid format.
.

;// 1: MAC address
MessageId=9010
SymbolicName=DEBUG_MAGIC_PACKET_DUPLICATE
Language=English
Received duplicate/excluded request for %%702 %1
.

;// 1: MAC address
MessageId=9011
SymbolicName=DEBUG_MAGIC_PACKET_EXCLUSION_ENDED
Language=English
Ending magic packet exclusion period for %%702 %1
.

;// 1: Job type code
;// 2: Job ID
MessageId=9012
SymbolicName=DEBUG_VIRTUALIZATION_JOB_RECEIVED
Language=English
Virtualization job created.
%%603 %%618: %1
%%603 %%602: %2
.

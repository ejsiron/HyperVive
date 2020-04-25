// **********
// Categories
// **********
//
//  Values are 32 bit values laid out as follows:
//
//   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
//   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
//  +---+-+-+-----------------------+-------------------------------+
//  |Sev|C|R|     Facility          |               Code            |
//  +---+-+-+-----------------------+-------------------------------+
//
//  where
//
//      Sev - is the severity code
//
//          00 - Success
//          01 - Informational
//          10 - Warning
//          11 - Error
//
//      C - is the Customer code flag
//
//      R - is a reserved bit
//
//      Facility - is the facility code
//
//      Code - is the facility's status code
//
//
// Define the facility codes
//


//
// Define the severity codes
//


//
// MessageId: CATEGORY_APPLICATION_ERROR
//
// MessageText:
//
// Application Error
//
#define CATEGORY_APPLICATION_ERROR       ((WORD)0x00000001L)

//
// MessageId: CATEGORY_MODULE_ERROR
//
// MessageText:
//
// Module Error
//
#define CATEGORY_MODULE_ERROR            ((WORD)0x00000002L)

//
// MessageId: CATEGORY_DEBUG_MESSAGE
//
// MessageText:
//
// Debug
//
#define CATEGORY_DEBUG_MESSAGE           ((WORD)0x00000003L)

//
// MessageId: CATEGORY_MAGIC_PACKET
//
// MessageText:
//
// Magic Packet
//
#define CATEGORY_MAGIC_PACKET            ((WORD)0x00000004L)

//
// MessageId: CATEGORY_CHECKPOINT
//
// MessageText:
//
// Checkpoint
//
#define CATEGORY_CHECKPOINT              ((WORD)0x00000005L)

// ************
// Module Names
// ************
//
// MessageId: MODULENAME_REGISTRY
//
// MessageText:
//
// %%619
//
#define MODULENAME_REGISTRY              ((WORD)0x000001F4L)

//
// MessageId: MODULENAME_WAKEONLAN
//
// MessageText:
//
// Wake-on-LAN
//
#define MODULENAME_WAKEONLAN             ((WORD)0x000001F5L)

// *******************
// Template Components
// *******************
//
// MessageId: TEMPLATE_COMPONENT_VIRTUAL_MACHINE
//
// MessageText:
//
// Virtual Machine%0
//
#define TEMPLATE_COMPONENT_VIRTUAL_MACHINE ((WORD)0x00000258L)

//
// MessageId: TEMPLATE_COMPONENT_IP_ADDRESS
//
// MessageText:
//
// IP Address%0
//
#define TEMPLATE_COMPONENT_IP_ADDRESS    ((WORD)0x00000259L)

//
// MessageId: TEMPLATE_COMPONENT_ID
//
// MessageText:
//
// ID%0
//
#define TEMPLATE_COMPONENT_ID            ((WORD)0x0000025AL)

//
// MessageId: TEMPLATE_COMPONENT_JOB
//
// MessageText:
//
// Job%
//
#define TEMPLATE_COMPONENT_JOB           ((WORD)0x0000025BL)

//
// MessageId: TEMPLATE_COMPONENT_CHECKPOINT_ACTION
//
// MessageText:
//
// Checkpoint action%0
//
#define TEMPLATE_COMPONENT_CHECKPOINT_ACTION ((WORD)0x0000025CL)

//
// MessageId: TEMPLATE_COMPONENT_STARTED
//
// MessageText:
//
// started%0
//
#define TEMPLATE_COMPONENT_STARTED       ((WORD)0x0000025DL)

//
// MessageId: TEMPLATE_COMPONENT_SUCCEEDED
//
// MessageText:
//
// succeeded%0
//
#define TEMPLATE_COMPONENT_SUCCEEDED     ((WORD)0x0000025EL)

//
// MessageId: TEMPLATE_COMPONENT_FAILED
//
// MessageText:
//
// failed%0
//
#define TEMPLATE_COMPONENT_FAILED        ((WORD)0x0000025FL)

//
// MessageId: TEMPLATE_COMPONENT_ERROR
//
// MessageText:
//
// Error%0
//
#define TEMPLATE_COMPONENT_ERROR         ((WORD)0x00000260L)

//
// MessageId: TEMPLATE_COMPONENT_RESULT
//
// MessageText:
//
// Result%0
//
#define TEMPLATE_COMPONENT_RESULT        ((WORD)0x00000261L)

//
// MessageId: TEMPLATE_COMPONENT_READ
//
// MessageText:
//
// Read%0
//
#define TEMPLATE_COMPONENT_READ          ((WORD)0x00000262L)

//
// MessageId: TEMPLATE_COMPONENT_OPEN
//
// MessageText:
//
// Open%0
//
#define TEMPLATE_COMPONENT_OPEN          ((WORD)0x00000263L)

//
// MessageId: TEMPLATE_COMPONENT_VIRTUAL_ADAPTER
//
// MessageText:
//
// Virtual network adapter%0
//
#define TEMPLATE_COMPONENT_VIRTUAL_ADAPTER ((WORD)0x00000264L)

//
// MessageId: TEMPLATE_COMPONENT_CREATED
//
// MessageText:
//
// created%0
//
#define TEMPLATE_COMPONENT_CREATED       ((WORD)0x00000265L)

//
// MessageId: TEMPLATE_COMPONENT_CHANGED
//
// MessageText:
//
// changed%0
//
#define TEMPLATE_COMPONENT_CHANGED       ((WORD)0x00000266L)

//
// MessageId: TEMPLATE_COMPONENT_DELETED
//
// MessageText:
//
// deleted%0
//
#define TEMPLATE_COMPONENT_DELETED       ((WORD)0x00000267L)

//
// MessageId: TEMPLATE_COMPONENT_EMULATED
//
// MessageText:
//
// emulated%0
//
#define TEMPLATE_COMPONENT_EMULATED      ((WORD)0x00000268L)

//
// MessageId: TEMPLATE_COMPONENT_SYNTHETIC
//
// MessageText:
//
// synthetic%0
//
#define TEMPLATE_COMPONENT_SYNTHETIC     ((WORD)0x00000269L)

//
// MessageId: TEMPLATE_COMPONENT_TYPE
//
// MessageText:
//
// type%0
//
#define TEMPLATE_COMPONENT_TYPE          ((WORD)0x0000026AL)

//
// MessageId: TEMPLATE_COMPONENT_REGISTRY
//
// MessageText:
//
// Registry%0
//
#define TEMPLATE_COMPONENT_REGISTRY      ((WORD)0x0000026BL)

//
// MessageId: TEMPLATE_COMPONENT_KVP
//
// MessageText:
//
// KVP%0
//
#define TEMPLATE_COMPONENT_KVP           ((WORD)0x0000026CL)

// *************
// Sub-templates
// *************
//
// MessageId: SUBTEMPLATE_VIRTUAL_MACHINE_NAME
//
// MessageText:
//
// %%600 name:%0
//
#define SUBTEMPLATE_VIRTUAL_MACHINE_NAME ((WORD)0x000002BCL)

//
// MessageId: SUBTEMPLATE_VIRTUAL_MACHINE_ID
//
// MessageText:
//
// %%600 ID:%0
//
#define SUBTEMPLATE_VIRTUAL_MACHINE_ID   ((WORD)0x000002BDL)

//
// MessageId: SUBTEMPLATE_MAC_ADDRESS
//
// MessageText:
//
// MAC address:%0
//
#define SUBTEMPLATE_MAC_ADDRESS          ((WORD)0x000002BEL)

//
// MessageId: SUBTEMPLATE_REQUEST_SOURCE
//
// MessageText:
//
// Request source%0
//
#define SUBTEMPLATE_REQUEST_SOURCE       ((WORD)0x000002BFL)

//
// MessageId: SUBTEMPLATE_RESULT_CODE
//
// MessageText:
//
// %%609 code:%0
//
#define SUBTEMPLATE_RESULT_CODE          ((WORD)0x000002C0L)

//
// MessageId: SUBTEMPLATE_RESULT_MESSAGE
//
// MessageText:
//
// %%609 message:%0
//
#define SUBTEMPLATE_RESULT_MESSAGE       ((WORD)0x000002C1L)

//
// MessageId: SUBTEMPLATE_INITIATOR
//
// MessageText:
//
// Initiator:%0
//
#define SUBTEMPLATE_INITIATOR            ((WORD)0x000002C2L)

//
// MessageId: SUBTEMPLATE_ERROR_MESSAGE
//
// MessageText:
//
// %%608 message:%0
//
#define SUBTEMPLATE_ERROR_MESSAGE        ((WORD)0x000002C3L)

//
// MessageId: SUBTEMPLATE_ERROR_TYPE
//
// MessageText:
//
// %%608 %%618:%0
//
#define SUBTEMPLATE_ERROR_TYPE           ((WORD)0x000002C4L)

//
// MessageId: SUBTEMPLATE_REGISTRY_ACCESS
//
// MessageText:
//
// %%619 access error.
// Action:%0
//
#define SUBTEMPLATE_REGISTRY_ACCESS      ((WORD)0x000002C5L)

//
// MessageId: SUBTEMPLATE_PATH
//
// MessageText:
//
// Path:%0
//
#define SUBTEMPLATE_PATH                 ((WORD)0x000002C6L)

//
// MessageId: SUBTEMPLATE_VIRTUAL_ADAPTER_TYPE
//
// MessageText:
//
// %%612 %%618:%0
//
#define SUBTEMPLATE_VIRTUAL_ADAPTER_TYPE ((WORD)0x000002C7L)

//
// MessageId: SUBTEMPLATE_REGISTRY_PATH
//
// MessageText:
//
// %%619 %%710: %0
//
#define SUBTEMPLATE_REGISTRY_PATH        ((WORD)0x000002C8L)

//
// MessageId: SUBTEMPLATE_VIRTUAL_MACHINE_BOOT_ATTEMPT
//
// MessageText:
//
// %%600 boot-up attempt%0
//
#define SUBTEMPLATE_VIRTUAL_MACHINE_BOOT_ATTEMPT ((WORD)0x000002C9L)

// ********
// Messages
// ********
// 1: Error message
// 2: Error type
//
// MessageId: APPLICATION_HALT_ERROR
//
// MessageText:
//
// Halting due to unexpected error.
// %%707 %1
// %%708 %2
//
#define APPLICATION_HALT_ERROR           ((WORD)0x000003E8L)

// 1: Module name
// 2: Error message
// 3: Error type
//
// MessageId: MODULE_ERROR
//
// MessageText:
//
// An unexpected error occurred.
// Module: %1
// %%707 %2
// %%708 %3
//
#define MODULE_ERROR                     ((WORD)0x000003E9L)

// 1: Registry path
// 2: Error message
// 3: Error type
//
// MessageId: REGISTRY_ACCESS_ERROR
//
// MessageText:
//
// %%709 %%610
// %%712 %1
// %%707 %2
// %%708 %3
//
#define REGISTRY_ACCESS_ERROR            ((WORD)0x000003F3L)

// 1: Registry path
// 2: Error message
// 3: Error type
//
// MessageId: REGISTRY_OPENKEY_ERROR
//
// MessageText:
//
// %%709 %%611
// %%712 %1
// %%707 %2
// %%708 %3
//
#define REGISTRY_OPENKEY_ERROR           ((WORD)0x000003F4L)

// 1: VNIC Instance ID
//
// MessageId: INVALID_VIRTUAL_ADAPTER
//
// MessageText:
//
// Invalid virtual network adapter instance ID %1.
//
#define INVALID_VIRTUAL_ADAPTER          ((WORD)0x000003FDL)

// No special errors at this time. Rolled up into 1001
// MessageId=1022
// SymbolicName=VIRTUAL_ADAPTER_SUBSCRIBER_ERROR
// Virtual adapter subscriber received an error.
// Type: %1
// Message: %2
// .
// 1: Target MAC address
// 2: Requesting IP address
//
// MessageId: MAGIC_PACKET_PROCESSED
//
// MessageText:
//
// Received wake-on-LAN frame
// %%702 %1
// %%703 %%601: %2.
//
#define MAGIC_PACKET_PROCESSED           ((WORD)0x000007D0L)

// 1: VM name
// 2: VM ID
// 3: VM MAC address
// 4: WOL request source IP
//
// MessageId: VIRTUAL_MACHINE_START_SUCCESS
//
// MessageText:
//
// %713 %%606.
// %%700 %1
// %%701 %2
// %%600 %%702 %3
// %%703 %%601: %4
//
#define VIRTUAL_MACHINE_START_SUCCESS    ((WORD)0x00000BB8L)

// 1: VM name
// 2: VM ID
// 3: VM MAC address
// 4: WOL request source IP
// 5: Result code
// 6: Result message
//
// MessageId: VIRTUAL_MACHINE_START_FAIL
//
// MessageText:
//
// %713 %%607.
// %%700 %1
// %%701 %2
// %%600 %%702 %3
// %%703 %%601: %4
// %%704: %5
// %%705: %6
//
#define VIRTUAL_MACHINE_START_FAIL       ((WORD)0x00000BB9L)

// 1: Checkpoint action
// 2: VM name
// 3: User name that started the job
// 4: VM ID
// 5: Job ID
//
// MessageId: CHECKPOINT_ACTION_STARTED
//
// MessageText:
//
// %%604 "%1" %%605
// %%600: %2
// %%706 %3
// %%600 %%701 %4
// %%603 %%701 %5
//
#define CHECKPOINT_ACTION_STARTED        ((WORD)0x00000FA0L)

// 1: Checkpoint action
// 2: VM name
// 3: User name that started the job
// 4: VM ID
// 5: Job ID
//
// MessageId: CHECKPOINT_ACTION_SUCCESS
//
// MessageText:
//
// %%604 "%1" %%606
// %%600: %2
// %%706 %3
// %%600 %%701 %4
// %%603 %%701 %5
//
#define CHECKPOINT_ACTION_SUCCESS        ((WORD)0x00000FA1L)

// 1: Checkpoint action
// 2: VM name
// 3: User name that started the job
// 4: VM ID
// 5: Job ID
// 6: Job error/result code
// 7: Job error/result message
//
// MessageId: CHECKPOINT_ACTION_FAIL
//
// MessageText:
//
// %%604 "%1" %%607
// %%600: %2
// %%706 %3
// %%600 %%701 %4
// %%603 %%701 %5
// %%704 %6
// %%705 %7
//
#define CHECKPOINT_ACTION_FAIL           ((WORD)0x00000FA2L)

// 1: Message as input
//
// MessageId: DEBUG_MESSAGE_GENERIC
//
// MessageText:
//
// %1
//
#define DEBUG_MESSAGE_GENERIC            ((WORD)0x00002328L)

// 1: Debug mode switch setting
//
// MessageId: DEBUG_DEBUG_MODE_CHANGED
//
// MessageText:
//
// Debug mode set to %1
//
#define DEBUG_DEBUG_MODE_CHANGED         ((WORD)0x00002329L)

// 1: KVP name
// 2: Registry path
//
// MessageId: DEBUG_REGISTRY_KVP_NOTFOUND
//
// MessageText:
//
// %%619 %%620 not found.
// %%620 name: %1
// %%710 %2
//
#define DEBUG_REGISTRY_KVP_NOTFOUND      ((WORD)0x0000232AL)

// 1: Number of virtual adapters
//
// MessageId: DEBUG_VIRTUAL_ADAPTER_ENUMERATED_COUNT
//
// MessageText:
//
// Enumerated %1 virtual network adapters.
//
#define DEBUG_VIRTUAL_ADAPTER_ENUMERATED_COUNT ((WORD)0x0000232BL)

// 1: MAC address
// 2: emulated or synthetic
//
// MessageId: DEBUG_VIRTUAL_ADAPTER_NEW
//
// MessageText:
//
// %%612 %%613
// %%702 %1
// %%711 %2
//
#define DEBUG_VIRTUAL_ADAPTER_NEW        ((WORD)0x0000232CL)

// 1: MAC address
// 2: emulated or synthetic
//
// MessageId: DEBUG_VIRTUAL_ADAPTER_CHANGED
//
// MessageText:
//
// %%612 %%614
// %%702 %1
// %%711 %2
//
#define DEBUG_VIRTUAL_ADAPTER_CHANGED    ((WORD)0x0000232DL)

// 1: MAC address
// 2: emulated or synthetic
//
// MessageId: DEBUG_VIRTUAL_ADAPTER_NEW_FROM_UPDATE
//
// MessageText:
//
// %%612 %%613 from update event
// %%702 %1
// %%711 %2
//
#define DEBUG_VIRTUAL_ADAPTER_NEW_FROM_UPDATE ((WORD)0x0000232EL)

// 1: MAC address
// 2: emulated or synthetic
//
// MessageId: DEBUG_VIRTUAL_ADAPTER_DELETED
//
// MessageText:
//
// %%612 %%615
// %%702 %1
// %%711 %2
//
#define DEBUG_VIRTUAL_ADAPTER_DELETED    ((WORD)0x0000232FL)

// 1: VM name
// 2: VM ID
// 3: Job ID
//
// MessageId: DEBUG_INITIATED_VM_START
//
// MessageText:
//
// Initiated %%600 boot-up
// %%600: %1
// %%600 %%602: %2
// %%603 %%602: %3
//
#define DEBUG_INITIATED_VM_START         ((WORD)0x00002330L)

//
// MessageId: DEBUG_MAGIC_PACKET_INVALID_FORMAT
//
// MessageText:
//
// Possible magic packet received with invalid format.
//
#define DEBUG_MAGIC_PACKET_INVALID_FORMAT ((WORD)0x00002331L)

// 1: MAC address
//
// MessageId: DEBUG_MAGIC_PACKET_DUPLICATE
//
// MessageText:
//
// Received duplicate/excluded request for %%702 %1
//
#define DEBUG_MAGIC_PACKET_DUPLICATE     ((WORD)0x00002332L)

// 1: MAC address
//
// MessageId: DEBUG_MAGIC_PACKET_EXCLUSION_ENDED
//
// MessageText:
//
// Ending magic packet exclusion period for %%702 %1
//
#define DEBUG_MAGIC_PACKET_EXCLUSION_ENDED ((WORD)0x00002333L)

// 1: Job type code
// 2: Job ID
//
// MessageId: DEBUG_VIRTUALIZATION_JOB_RECEIVED
//
// MessageText:
//
// Virtualization job created.
// %%603 %%618: %1
// %%603 %%602: %2
//
#define DEBUG_VIRTUALIZATION_JOB_RECEIVED ((WORD)0x00002334L)


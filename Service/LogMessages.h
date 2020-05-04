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
#define CATEGORY_APPLICATION_ERROR       ((WORD)1L)

//
// MessageId: CATEGORY_MODULE_ERROR
//
// MessageText:
//
// Module Error
//
#define CATEGORY_MODULE_ERROR            ((WORD)2L)

//
// MessageId: CATEGORY_DEBUG_MESSAGE
//
// MessageText:
//
// Debug
//
#define CATEGORY_DEBUG_MESSAGE           ((WORD)3L)

//
// MessageId: CATEGORY_MAGIC_PACKET
//
// MessageText:
//
// Magic Packet
//
#define CATEGORY_MAGIC_PACKET            ((WORD)4L)

//
// MessageId: CATEGORY_VM_STARTER
//
// MessageText:
//
// Virtual Machine Starter
//
#define CATEGORY_VM_STARTER              ((WORD)5L)

//
// MessageId: CATEGORY_CHECKPOINT
//
// MessageText:
//
// Checkpoint
//
#define CATEGORY_CHECKPOINT              ((WORD)6L)

//
// MessageId: CATEGORY_CIM_ERROR
//
// MessageText:
//
// CIM/WMI Error
//
#define CATEGORY_CIM_ERROR               ((WORD)7L)

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
#define MODULENAME_REGISTRY              ((WORD)500L)

//
// MessageId: MODULENAME_WAKEONLAN
//
// MessageText:
//
// Wake-on-LAN
//
#define MODULENAME_WAKEONLAN             ((WORD)501L)

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
#define TEMPLATE_COMPONENT_VIRTUAL_MACHINE ((WORD)600L)

//
// MessageId: TEMPLATE_COMPONENT_IP_ADDRESS
//
// MessageText:
//
// IP Address%0
//
#define TEMPLATE_COMPONENT_IP_ADDRESS    ((WORD)601L)

//
// MessageId: TEMPLATE_COMPONENT_ID
//
// MessageText:
//
// ID%0
//
#define TEMPLATE_COMPONENT_ID            ((WORD)602L)

//
// MessageId: TEMPLATE_COMPONENT_JOB
//
// MessageText:
//
// Job%
//
#define TEMPLATE_COMPONENT_JOB           ((WORD)603L)

//
// MessageId: TEMPLATE_COMPONENT_CHECKPOINT_ACTION
//
// MessageText:
//
// Checkpoint action%0
//
#define TEMPLATE_COMPONENT_CHECKPOINT_ACTION ((WORD)604L)

//
// MessageId: TEMPLATE_COMPONENT_STARTED
//
// MessageText:
//
// started%0
//
#define TEMPLATE_COMPONENT_STARTED       ((WORD)605L)

//
// MessageId: TEMPLATE_COMPONENT_SUCCEEDED
//
// MessageText:
//
// succeeded%0
//
#define TEMPLATE_COMPONENT_SUCCEEDED     ((WORD)606L)

//
// MessageId: TEMPLATE_COMPONENT_FAILED
//
// MessageText:
//
// failed%0
//
#define TEMPLATE_COMPONENT_FAILED        ((WORD)607L)

//
// MessageId: TEMPLATE_COMPONENT_ERROR
//
// MessageText:
//
// Error%0
//
#define TEMPLATE_COMPONENT_ERROR         ((WORD)608L)

//
// MessageId: TEMPLATE_COMPONENT_RESULT
//
// MessageText:
//
// Result%0
//
#define TEMPLATE_COMPONENT_RESULT        ((WORD)609L)

//
// MessageId: TEMPLATE_COMPONENT_READ
//
// MessageText:
//
// Read%0
//
#define TEMPLATE_COMPONENT_READ          ((WORD)610L)

//
// MessageId: TEMPLATE_COMPONENT_OPEN
//
// MessageText:
//
// Open%0
//
#define TEMPLATE_COMPONENT_OPEN          ((WORD)611L)

//
// MessageId: TEMPLATE_COMPONENT_VIRTUAL_ADAPTER
//
// MessageText:
//
// Virtual network adapter%0
//
#define TEMPLATE_COMPONENT_VIRTUAL_ADAPTER ((WORD)612L)

//
// MessageId: TEMPLATE_COMPONENT_CREATED
//
// MessageText:
//
// created%0
//
#define TEMPLATE_COMPONENT_CREATED       ((WORD)613L)

//
// MessageId: TEMPLATE_COMPONENT_CHANGED
//
// MessageText:
//
// changed%0
//
#define TEMPLATE_COMPONENT_CHANGED       ((WORD)614L)

//
// MessageId: TEMPLATE_COMPONENT_DELETED
//
// MessageText:
//
// deleted%0
//
#define TEMPLATE_COMPONENT_DELETED       ((WORD)615L)

//
// MessageId: TEMPLATE_COMPONENT_EMULATED
//
// MessageText:
//
// emulated%0
//
#define TEMPLATE_COMPONENT_EMULATED      ((WORD)616L)

//
// MessageId: TEMPLATE_COMPONENT_SYNTHETIC
//
// MessageText:
//
// synthetic%0
//
#define TEMPLATE_COMPONENT_SYNTHETIC     ((WORD)617L)

//
// MessageId: TEMPLATE_COMPONENT_TYPE
//
// MessageText:
//
// type%0
//
#define TEMPLATE_COMPONENT_TYPE          ((WORD)618L)

//
// MessageId: TEMPLATE_COMPONENT_REGISTRY
//
// MessageText:
//
// Registry%0
//
#define TEMPLATE_COMPONENT_REGISTRY      ((WORD)619L)

//
// MessageId: TEMPLATE_COMPONENT_KVP
//
// MessageText:
//
// KVP%0
//
#define TEMPLATE_COMPONENT_KVP           ((WORD)620L)

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
#define SUBTEMPLATE_VIRTUAL_MACHINE_NAME ((WORD)700L)

//
// MessageId: SUBTEMPLATE_VIRTUAL_MACHINE_ID
//
// MessageText:
//
// %%600 ID:%0
//
#define SUBTEMPLATE_VIRTUAL_MACHINE_ID   ((WORD)701L)

//
// MessageId: SUBTEMPLATE_MAC_ADDRESS
//
// MessageText:
//
// MAC address:%0
//
#define SUBTEMPLATE_MAC_ADDRESS          ((WORD)702L)

//
// MessageId: SUBTEMPLATE_REQUEST_SOURCE
//
// MessageText:
//
// Request source%0
//
#define SUBTEMPLATE_REQUEST_SOURCE       ((WORD)703L)

//
// MessageId: SUBTEMPLATE_RESULT_CODE
//
// MessageText:
//
// %%609 code:%0
//
#define SUBTEMPLATE_RESULT_CODE          ((WORD)704L)

//
// MessageId: SUBTEMPLATE_RESULT_MESSAGE
//
// MessageText:
//
// %%609 message:%0
//
#define SUBTEMPLATE_RESULT_MESSAGE       ((WORD)705L)

//
// MessageId: SUBTEMPLATE_INITIATOR
//
// MessageText:
//
// Initiator:%0
//
#define SUBTEMPLATE_INITIATOR            ((WORD)706L)

//
// MessageId: SUBTEMPLATE_ERROR_MESSAGE
//
// MessageText:
//
// %%608 message:%0
//
#define SUBTEMPLATE_ERROR_MESSAGE        ((WORD)707L)

//
// MessageId: SUBTEMPLATE_ERROR_TYPE
//
// MessageText:
//
// %%608 %%618:%0
//
#define SUBTEMPLATE_ERROR_TYPE           ((WORD)708L)

//
// MessageId: SUBTEMPLATE_REGISTRY_ACCESS
//
// MessageText:
//
// %%619 access error.
// Action:%0
//
#define SUBTEMPLATE_REGISTRY_ACCESS      ((WORD)709L)

//
// MessageId: SUBTEMPLATE_PATH
//
// MessageText:
//
// Path:%0
//
#define SUBTEMPLATE_PATH                 ((WORD)710L)

//
// MessageId: SUBTEMPLATE_VIRTUAL_ADAPTER_TYPE
//
// MessageText:
//
// %%612 %%618:%0
//
#define SUBTEMPLATE_VIRTUAL_ADAPTER_TYPE ((WORD)711L)

//
// MessageId: SUBTEMPLATE_REGISTRY_PATH
//
// MessageText:
//
// %%619 %%710: %0
//
#define SUBTEMPLATE_REGISTRY_PATH        ((WORD)712L)

//
// MessageId: SUBTEMPLATE_VIRTUAL_MACHINE_BOOT_ATTEMPT
//
// MessageText:
//
// %%600 boot-up attempt%0
//
#define SUBTEMPLATE_VIRTUAL_MACHINE_BOOT_ATTEMPT ((WORD)713L)

//
// MessageId: SUBTEMPLATE_MODULE
//
// MessageText:
//
// Module:%0
//
#define SUBTEMPLATE_MODULE               ((WORD)714L)

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
#define APPLICATION_HALT_ERROR           ((WORD)1000L)

// 1: Module name
// 2: Error message
// 3: Error type
//
// MessageId: MODULE_ERROR
//
// MessageText:
//
// An unexpected error occurred.
// %%714 %1
// %%707 %2
// %%708 %3
//
#define MODULE_ERROR                     ((WORD)1001L)

// 1: Error message
// 2: Module name
//
// MessageId: CIM_ERROR
//
// MessageText:
//
// An error occurred in the CIM/WMI subsystem.
// %%707 %1
// %%714 %2
//
#define CIM_ERROR                        ((WORD)1002L)

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
#define REGISTRY_ACCESS_ERROR            ((WORD)1011L)

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
#define REGISTRY_OPENKEY_ERROR           ((WORD)1012L)

// 1: VNIC Instance ID
//
// MessageId: INVALID_VIRTUAL_ADAPTER
//
// MessageText:
//
// Invalid virtual network adapter instance ID %1.
//
#define INVALID_VIRTUAL_ADAPTER          ((WORD)1021L)

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
#define MAGIC_PACKET_PROCESSED           ((WORD)2000L)

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
#define VIRTUAL_MACHINE_START_SUCCESS    ((WORD)3000L)

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
#define VIRTUAL_MACHINE_START_FAIL       ((WORD)3001L)

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
#define CHECKPOINT_ACTION_STARTED        ((WORD)4000L)

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
#define CHECKPOINT_ACTION_SUCCESS        ((WORD)4001L)

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
#define CHECKPOINT_ACTION_FAIL           ((WORD)4002L)

// 1: Message as input
// 2: Module name
//
// MessageId: DEBUG_MESSAGE_GENERIC
//
// MessageText:
//
// %1
// %%714 %2
//
#define DEBUG_MESSAGE_GENERIC            ((WORD)9000L)

// 1: Debug mode switch setting
//
// MessageId: DEBUG_DEBUG_MODE_CHANGED
//
// MessageText:
//
// Debug mode set to %1
//
#define DEBUG_DEBUG_MODE_CHANGED         ((WORD)9001L)

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
#define DEBUG_REGISTRY_KVP_NOTFOUND      ((WORD)9002L)

// 1: Number of virtual adapters
//
// MessageId: DEBUG_VIRTUAL_ADAPTER_ENUMERATED_COUNT
//
// MessageText:
//
// Enumerated %1 virtual network adapters.
//
#define DEBUG_VIRTUAL_ADAPTER_ENUMERATED_COUNT ((WORD)9003L)

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
#define DEBUG_VIRTUAL_ADAPTER_NEW        ((WORD)9004L)

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
#define DEBUG_VIRTUAL_ADAPTER_CHANGED    ((WORD)9005L)

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
#define DEBUG_VIRTUAL_ADAPTER_NEW_FROM_UPDATE ((WORD)9006L)

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
#define DEBUG_VIRTUAL_ADAPTER_DELETED    ((WORD)9007L)

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
#define DEBUG_INITIATED_VM_START         ((WORD)9008L)

//
// MessageId: DEBUG_MAGIC_PACKET_INVALID_FORMAT
//
// MessageText:
//
// Possible magic packet received with invalid format.
//
#define DEBUG_MAGIC_PACKET_INVALID_FORMAT ((WORD)9009L)

// 1: MAC address
//
// MessageId: DEBUG_MAGIC_PACKET_DUPLICATE
//
// MessageText:
//
// Received duplicate/excluded request for %%702 %1
//
#define DEBUG_MAGIC_PACKET_DUPLICATE     ((WORD)9010L)

// 1: MAC address
//
// MessageId: DEBUG_MAGIC_PACKET_EXCLUSION_ENDED
//
// MessageText:
//
// Ending magic packet exclusion period for %%702 %1
//
#define DEBUG_MAGIC_PACKET_EXCLUSION_ENDED ((WORD)9011L)

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
#define DEBUG_VIRTUALIZATION_JOB_RECEIVED ((WORD)9012L)


# HyperVive

A service that performs utility functions for Hyper-V virtual machines

Current version: 3.0

## Version 2.0 feature: Checkpoint Watcher

Want to know WHO created that Hyper-V checkpoint (formerly known as snapshots)? Now you can find out! HyperVive will watch for checkpointing events and record details. It records one event for the initiation of a checkpoint job and another when it completes.

### Checkpoint Watcher Technical Brief

Hyper-V uses WMI jobs behind the scenes for most of its activities. The job object contains details, including the user name of the individual that requested the job. So, HyperVive watches the WMI space for new jobs to appear. When one does, HyperVive checks to see if it has anything to do with a checkpoint. If it does, then HyperVive records the start of the event. It then watches for the event to end, and records the outcome of the job.

HyperVive will record events related to these types of WMI jobs:

- Checkpoint creation
- Checkpoint deletion
- Checkpoint application (revert)
- Clearing of a checkpoint's saved state

You will find these event entries in the host's regular Application event log.

## Version 1.0 feature: Wake-On-LAN

HyperVive intercepts wake-on-LAN frames and starts all matching virtual machines from Off, Saved, or Paused states.

### Wake-on-LAN Technical Brief

HyperVive maintains a local inventory of all virtual adapters connected to virtual machines (synthetic or emulated). It continually watches for adds, changes, and deletes. These events occur when virtual machines are created, migrated on or off the host, and when individual adapters are added, removed, or changed on virtual machines.

HyperVive listens on all _host_ network adapters (physical or virtual). This means that the management operating system does need a presence in all VLANs that might carry WOL frames. It cannot listen on VLANs that only virtual machines connect to.

Data flow:

1. HyperVive intercepts a WOL frame and validates its format
2. HyperVive compares the MAC in the WOL frame to to all known virtual adapter MACs
3. If any MACs match, HyperVive checks that the connecting virtual machine is in an Off, Saved, or Paused state
4. HyperVive starts any virtual machine that passes all of the previous checks
5. HyperVive logs matched WOL frames and the outcome of start events to the Application log

Newly-created virtual adapters set to use a dynamic MAC (like those on a newly-created VM) will have a MAC of 00:00:00:00:00:00. If HyperVive receives a WOL frame for that MAC, it will start all applicable VMs.

## Debug Mode

To instruct HyperVive to log detailed messages for troubleshooting, make the following registry change:

- **Path**: HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\HyperVive
- **Value name**: DebugMode
- **Value type**: DWORD
- **Value**: 1 for debug logging, 0 for minimal logging (default)

Changes take effect instantly; you do not need to restart the service. If you create and then delete the key, it will stay in its current mode until restarted.

### Use PowerShell to set Debug Mode

To enable in PowerShell:

```PowerShell
Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\HyperVive\ -Name DebugMode -Value 1
```

To disable in PowerShell:

```PowerShell
Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\HyperVive\ -Name DebugMode -Value 0
```

Changes take effect instantly; you do not need to restart the service.

## Installation and Removal Instructions

Use the latest SetupHyperVive.msi from the [Releases page](https://github.com/ejsiron/HyperVive/releases) to install, remove, or upgrade.

*NOTE*: The MSI can cleanly upgrade manually-installed instances of versions 1 and 2.

## HyperVive Event IDs

The following reference contains all HyperVive event ID codes and their basic meanings. Check the specific event for details.

- 1000: Unexpected application error
- 1001: Module-specific error
- 1011: Registry access error
- 1012: Registry key open error
- 1021: Invalid virtual adapter
- 1022: Virtual adapter subscriber error
- 1031: Wake-on-LAN receiver error
- 2000: Magic packet received and processed
- 3000: Successfully started a virtual machine
- 3001: Failed to start a virtual machine
- 4000: A checkpoint-related job has started
- 4001: A checkpoint-related job succeeded
- 4002: A checkpoint-related job failed
- 9000: Non-specific debug message
- 9001: HyperVive's Debug mode changed
- 9002: Debug Mode registry key not found
- 9003: Enumerated virtual adapters (debug)
- 9004: A new virtual adapter was created (debug)
- 9005: A virtual adapter changed (debug)
- 9006: An update to a virtual adapter was intercepted, but HyperVive did not know it existed (debug)
- 9007: A virtual adapter was deleted (debug)
- 9008: HyperVive is trying to start a virtual machine (debug)
- 9009: Something that looked like a magic packet was received, but it turned out to be malformed (debug)
- 9010: Magic packet received, even if HyperVive chose not to process it (debug)
- 9011: MAC exclusion period ended (debug)
- 9012: Virtualization-related job intercepted (debug)

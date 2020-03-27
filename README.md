# HyperVive

A service that performs utility functions for Hyper-V virtual machines

Current version: 1.0.1

## Version 1.0 feature: Wake-On-LAN

HyperVive intercepts wake-on-LAN frames and starts all matching virtual machines from Off, Saved, or Paused states.

### Technical Brief

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

## Installation Instructions

Copy _HyperVive.exe_ to a suitable location. _Recommended_: C:\Program Files (x86)\HyperVive

Run the following at an elevated PowerShell prompt (make adjustments to the path as necessary):

```PowerShell
C:\windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe 'C:\Program Files (x86)\HyperVive\HyperVive.exe'
Start-Service HyperVive
```

## Removal Instructions

Run the following (make adjustments to the path as necessary)

```PowerShell
Stop-Service HyperVive
C:\windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe /u 'C:\Program Files (x86)\HyperVive\HyperVive.exe'
```

Delete the folder that you created to hold HyperVive.exe. You may also need to delete the registry key `HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\HyperVive`.

## Future Enhancements

I plan to add the following:

- An event recorder for checkpoints: WHO created them
- A proper MSI installer

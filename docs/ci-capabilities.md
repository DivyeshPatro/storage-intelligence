# CI test-volume capabilities (M0b feasibility probe)

Source: `.github/workflows/volume-probe.yml` → `tools/ci/Probe-VolumeCapabilities.ps1`, run 38700259465
(2026-10-15) on GitHub-hosted **windows-2025** (Windows Server 2025 Datacenter, build 26100) and **windows-2022**
(Windows Server 2022 Datacenter, build 20348). Results were identical on both images.

Test volumes are 2–4 GiB expandable VHDX files created with `diskpart`, initialized as GPT and formatted with
`Format-Volume`. They are created only when `GITHUB_ACTIONS=true` and `SI_ALLOW_TEST_VOLUMES=1`, are labelled
`SI<filesystem>`, carry a `.si-disposable-volume` marker, and are detached and deleted after each job.

| Feature | NTFS | ReFS | FAT32 | exFAT |
|---|---|---|---|---|
| Volume can be created and formatted | yes (4 KiB clusters) | yes (4 KiB) | yes (4 KiB) | yes (32 KiB) |
| Stable file IDs | yes | yes (128-bit) | synthesized only; not used | synthesized only; not used |
| Hard links | yes | yes | **no** | **no** |
| Junctions | yes | yes | **no** | **no** |
| Symbolic links | yes (runner is administrator) | yes | **no** | **no** |
| Sparse files | yes | yes | **no** | **no** |
| NTFS compression | yes | **no** | **no** | **no** |
| WOF compression | **only after `fltmc attach wof <volume>`** (filter not attached to new volumes by default) | **no** | **no** | **no** |
| Cloud-files placeholders | **not testable** (needs a sync provider) | not testable | n/a | n/a |
| ReFS block cloning | n/a | **not probed**; treated as uncertain by design | n/a | n/a |
| Alternate data streams | yes | yes | no | no |
| Deny-ACL inaccessible folders | yes | yes | **no** (no ACLs) | **no** |

## Notes

- **WOF hides itself.** On a WOF-backed file, `fsutil reparsepoint query` reports "not a reparse point", and the
  directory listing shows no reparse attribute and **allocation 0**. Only the WOF API (`WofIsExternalFile`) or
  `FSCTL_GET_EXTERNAL_BACKING` identifies it. See m0b-validation-results.md.
- **Hosted runners are Windows Server, not Windows 11.** R1 targets Windows 11 x64; client-specific behaviour (for
  example Dev Drive, OneDrive Files On-Demand) is not covered by CI and remains unvalidated.
- Hosted runners have SSD-backed storage and are shared; no performance conclusion may be drawn from them.
- Artifacts and job logs from this session could not be downloaded (the session's network policy blocks the
  artifact storage host), so CI scripts also publish results as check-run annotations
  (`tools/ci/run-annotations.sh <run-id>`). Artifacts remain available in the GitHub UI for the retention period.

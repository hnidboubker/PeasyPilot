# Security Policy

We take the security of PeasyPilot seriously and appreciate responsible disclosure from the community.

## Supported versions

The project currently targets .NET 8, .NET 9, and .NET 10. We encourage users to stay on the latest available patch release of the supported framework and the latest published package version of PeasyPilot.

We typically provide security fixes for:

- the current main branch
- the latest released version of each maintained package

If you are using an older release, we strongly recommend upgrading before reporting a vulnerability unless the issue is only reproducible on the older version.

## Reporting a vulnerability

Please do not open a public GitHub issue for security vulnerabilities.

Instead, report the issue privately by:

1. emailing the maintainers through the contact address listed in the repository metadata, or
2. using the repository's private security reporting channel if one is configured in the GitHub project settings.

When reporting, please include:

- a clear description of the vulnerability
- affected package, version, and target framework
- steps to reproduce the issue
- expected vs. actual behavior
- any proof-of-concept code or sample payloads
- your contact information for follow-up

## Disclosure timeline

We aim to acknowledge valid reports within 5 business days and to provide a remediation plan as soon as practical.

We ask reporters to allow us a reasonable period to investigate and patch the issue before publicly disclosing details. In most cases, we will coordinate disclosure with the reporter when a fix is ready.

## Security expectations

To help keep the project secure:

- use the latest supported package versions
- avoid publishing secrets or credentials in issue reports or test data
- validate dependencies before shipping build or release artifacts
- report suspected vulnerabilities privately rather than through public discussion

## Scope

This policy applies to the project source code, NuGet packages, build and release workflows, and repository configuration maintained in this repository.

If a vulnerability is found in a third-party dependency, we will coordinate responsibly with the upstream project and apply the least disruptive mitigation available.

## Thank you

Responsible disclosure helps protect users of PeasyPilot and keeps the ecosystem safer for everyone.

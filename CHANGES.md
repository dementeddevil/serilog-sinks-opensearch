# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

- Replace `Serilog.Sinks.PeriodicBatching` with Serilog 4.4 native batching for non-durable sinks.
- Add nullable `RetryTimeLimit` configuration and native temporary, permanent, and final failure reporting/fallback support.
- Whole-batch transport failures now propagate for retry; legacy failure handlers remain for rejected items in valid bulk responses.
- Preserve direct-construction source compatibility and the existing durable file-buffering path while removing the compatibility package dependency.

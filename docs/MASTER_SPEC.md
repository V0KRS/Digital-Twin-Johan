“Reference only; CLAUDE.md governs.” Sessions will read it only when a task needs it.

Checkpoint 0 approved. Checkpoint 1a complete. For “continue”, read CONTINUATION.md, follow Section 22, and commit and push to main at the end of every increment.



## 1. YOUR ROLE AND RESPONSIBILITIES

You are the principal software architect, senior full-stack engineer, 3D graphics engineer, industrial automation specialist, SCADA integration engineer, cybersecurity engineer, UI/UX designer, QA engineer, and technical documentation writer for this project.

Your task is to design, implement, test, document, and progressively deliver a professional industrial Digital Twin Platform suitable for real engineering use.

You must think and work like an experienced software engineering team building a commercial-quality internal engineering product. Do not approach this as a simple coding exercise, university project, visual mockup, or proof of concept that only looks impressive.

You are responsible for the quality of the actual software, not just the quality of your explanations.

You must produce real source code, a coherent application, working features, meaningful tests, documentation, and reproducible installation and deployment procedures.

Do not merely describe what should be implemented. Implement it.

Do not attempt to generate the entire product in a single enormous response. Work incrementally, with each stage delivering a useful, working and verifiable improvement to the existing application.

I will provide only one continuation instruction: **"continue"**.

Whenever I say "continue", you must independently inspect the current state of the project, determine the most appropriate next unit of work, and implement it. You must not require me to remember checkpoint numbers, repeat previous instructions, or specify which feature should come next.

These continuation rules are mandatory and are explained in detail below.

---

## 2. BUSINESS CONTEXT AND PRODUCT VISION

My company develops digital twins and SCADA-related systems, primarily for PUB projects in Singapore, including water treatment plants and related industrial infrastructure.

A recurring problem is that our engineers repeatedly build digital twins from scratch for different projects.

I want to create an internal software platform that standardises and automates as much of this engineering work as possible.

The software should allow engineers to import 3D models, assemble a virtual facility, configure equipment, connect SCADA tags, define visual behaviours, display live telemetry, configure alarms, view historical trends, save project configurations, and reuse existing components across future projects.

The long-term goal is to transform digital twin development from a mostly custom, project-by-project process into a reusable, configurable engineering workflow.

For example, an engineer starting a new water treatment plant project should ideally be able to:

1. Create a new project from a template.
2. Import the relevant 3D models.
3. Organise the models into the facility's equipment hierarchy.
4. Place reusable pumps, valves, tanks, sensors and other components.
5. Map SCADA tags to the appropriate equipment.
6. Configure equipment states, animations, alarms and telemetry displays.
7. Validate the project using simulated data.
8. Connect the project to an authorised SCADA data source.
9. Test the complete digital twin.
10. Save, package and deploy the project.

The engineer should not have to recreate the core application, telemetry system, equipment behaviours, alarm system, tag mapping interface or project infrastructure for every new client.

The platform must separate reusable software capabilities from project-specific models, configurations and data.

Design the application to support multiple independent projects, each with its own configuration and assets, while allowing approved shared templates and components to be reused.

Do not assume that every PUB project uses the same SCADA vendor, tag naming convention, equipment arrangement, network architecture or deployment environment.

The product must be configurable and extensible.

---

## 3. (May skip since stack is already decided) YOUR FIRST RESPONSIBILITY: CHOOSE THE RIGHT TECHNOLOGY

Before implementing the application, evaluate the most suitable technology and architecture.

Do not blindly choose a technology simply because it is popular or familiar.

Consider at least the following options:

* Unity with C#.
* A browser-based 3D engine such as Babylon.js or Three.js.
* A hybrid architecture combining a 3D engine, web-based configuration interface and backend services.
* Other technologies only if they provide a meaningful advantage.

Compare the options based on:

* 3D rendering capabilities.
* Importing and managing engineering models.
* Interactive scene editing.
* Equipment animations.
* Reusable components and prefabs.
* Real-time SCADA integration.
* Browser accessibility.
* Deployment on ordinary engineering workstations.
* Offline operation.
* Compatibility with restricted industrial networks.
* Performance with complex models.
* Ease of development and maintenance.
* Licensing and commercial distribution.
* Ease of packaging and handing over completed projects.
* Suitability for a small engineering team maintaining the platform over several years.

Make a clear recommendation and explain the most important trade-offs.

Do not spend excessive time researching alternatives or repeatedly reconsidering the stack after making a reasonable decision.

Use current, authoritative technical documentation when external research is available. Verify important compatibility, licensing and security assumptions instead of inventing facts.

If the selected technology has a significant limitation, address it explicitly.

### Preferred architectural direction

Unless your evaluation identifies a stronger alternative, consider:

* **3D client:** Unity with C# for a rich desktop engineering application, or a browser-based 3D engine for a browser-first solution.
* **Backend:** ASP.NET Core with C#.
* **Database:** PostgreSQL for projects, metadata, configuration and application records.
* **Industrial connectivity:** OPC UA and MQTT adapters, with additional connectors implemented through an extensible integration framework.
* **Real-time distribution:** WebSockets, SignalR or an appropriate equivalent.
* **Historical telemetry:** PostgreSQL initially, with a suitable time-series extension or separate storage solution if justified by measured requirements.
* **Deployment:** Docker for compatible backend services, with native application packaging where required.
* **Testing:** Automated unit, integration and end-to-end tests using appropriate tools for the selected stack.
* **Version control:** Git with a clear branching and checkpoint strategy.

This is a starting recommendation, not an instruction to force every technology into the product.

Do not introduce unnecessary microservices, databases, brokers or infrastructure. Prefer a modular architecture with clear boundaries, and split services only when the complexity or deployment requirements justify it.

Once you choose the architecture, record it in the project documentation and maintain consistency unless a genuine technical issue requires a change.

---

## 4. THE CORE APPLICATION

Build a professional application called **Industrial Digital Twin Studio**. The name can be changed later.

The application should provide an engineering workspace where users can create, edit, configure, validate, simulate and run digital twin projects.

It should have two clearly separated experiences:

### A. Digital Twin Editor

Used by engineers to create and configure projects.

It should include:

* Project management.
* 3D model import.
* Scene editing.
* Equipment hierarchy.
* Asset library.
* Equipment properties.
* Tag mapping.
* Animation and state configuration.
* Alarm configuration.
* Template management.
* Validation.
* Simulation and testing.
* Import/export and deployment preparation.

### B. Digital Twin Runtime

Used to operate, demonstrate and monitor a completed digital twin.

It should include:

* Interactive 3D visualisation.
* Equipment hierarchy.
* Live telemetry.
* Equipment state displays.
* Alarm monitoring.
* Historical trends.
* Equipment detail panels.
* Connection health.
* Search and filtering.
* Full-screen and presentation modes.
* Clear live/simulation mode indication.

The runtime should not expose development-only controls unnecessarily.

The editor and runtime may share common modules, but their responsibilities and permissions should be clearly separated.

---

## 5. PROJECT MANAGEMENT AND PERSISTENCE

Implement:

* Create, open, rename, duplicate, archive and delete projects, with appropriate confirmation and recovery safeguards.
* Project descriptions, identifiers, versions and metadata.
* Project templates.
* Save and load.
* Autosave where appropriate.
* Recovery from interrupted saves.
* Project import and export.
* Versioned configuration formats.
* Configuration migration between supported versions.
* Asset organisation and dependency tracking.
* Validation before project import or publication.
* Project comparison or configuration-diff functionality where practical.
* Backup and restoration.
* Clear project directory structures.
* Portable project packages that do not depend on hardcoded paths on one developer's machine.

Persist all important project configuration.

A project must remain usable after closing and restarting the application.

Do not rely on temporary in-memory state for essential information.

Implement transactional or otherwise reliable save operations where appropriate. Avoid silently corrupting or partially overwriting existing projects.

---

## 6. 3D MODEL IMPORT AND SCENE EDITOR

Engineers must be able to import existing 3D models rather than recreate them manually.

Evaluate and implement the most appropriate import workflow for formats such as:

* GLB/glTF.
* FBX.
* OBJ.
* IFC/BIM models where practical.
* Other formats if they are relevant to the chosen architecture.

Some formats may require external conversion tools. Document those requirements and provide a practical conversion workflow where direct import is unavailable.

The editor should support:

* Model import and preview.
* Model validation.
* Scene hierarchy.
* Object selection.
* Move, rotate and scale.
* Duplicate and delete.
* Rename and grouping.
* Multi-selection where practical.
* Visibility controls.
* Material and texture handling.
* Object properties.
* Model positioning and origin adjustment.
* Coordinate system and unit conversion where appropriate.
* Camera orbit, pan and zoom.
* Useful camera presets.
* Object highlighting.
* Labels and tooltips.
* Scene save and restore.
* Asset replacement.
* Import progress and meaningful errors.
* Asset thumbnails where practical.
* Model optimisation for large scenes.
* Instancing, level of detail and related performance techniques when useful.

Preserve the relationship between imported source assets and the objects configured in the project.

When an engineer replaces an updated 3D model, avoid forcing them to recreate unrelated equipment mappings, behaviours and metadata.

Provide a safe model-update workflow, identifying which parts of the configuration can be retained and which require manual review.

Do not claim support for a model format until its actual import workflow has been implemented and tested.

---

## 7. REUSABLE INDUSTRIAL EQUIPMENT LIBRARY

Create a reusable, searchable library of industrial assets.

Include configurable component types such as:

* Pumps.
* Motors.
* Manual and actuated valves.
* Tanks and reservoirs.
* Pipes and pipelines.
* Flow meters.
* Level sensors.
* Pressure sensors.
* Temperature sensors.
* Water-quality instrumentation.
* Conveyors where appropriate.
* Generic machines.
* Electrical and instrumentation equipment.

Each reusable equipment definition should support:

* 3D representation.
* Equipment type.
* Unique identifier.
* Display name.
* Engineering metadata.
* Editable properties.
* Optional manufacturer and model information.
* Tag mappings.
* Supported states.
* Animation rules.
* Alarm associations.
* Engineering units and valid ranges.
* Default visual settings.
* Optional documentation.
* Reusable behaviour definitions.

Distinguish between an equipment definition and an individual equipment instance.

For example, an engineer may create one reusable pump definition and place 20 pump instances in a facility. Each instance must be able to have its own name, SCADA tags, location, operating state and project-specific configuration.

Changing one instance must not unexpectedly modify every other instance.

Changing a library definition must not silently overwrite project-specific values.

Implement reusable prefabs, component templates or their equivalent in the selected technology.

Provide mechanisms to update library assets while identifying incompatible or breaking changes.

The equipment library should become one of the main mechanisms for reducing repeated engineering work.

---

## 8. CONFIGURABLE EQUIPMENT BEHAVIOURS AND ANIMATIONS

Build a flexible behaviour system.

Avoid hardcoding every pump, valve or tank directly into the application.

Equipment behaviours should be configurable and reusable.

### Pump example

Support states such as:

* Running.
* Stopped.
* Starting.
* Stopping.
* Faulted.
* Communication lost.
* Unknown.

Provide configurable visual behaviours such as rotating a motor or impeller representation when the pump is running.

### Valve example

Support:

* Open.
* Closed.
* Opening.
* Closing.
* Faulted.
* Unknown.

Allow valve position or animation to be linked to an actual position value where available.

### Tank example

Support:

* Tank level.
* Engineering units.
* High and low thresholds.
* High-high and low-low alarms.
* Configurable filling and draining visualisation.

Only display an animated water level when the relevant value or explicitly labelled simulation is available.

### Pipeline example

Support:

* Flow status.
* Direction of flow where available.
* Flow rate.
* Configurable state indication.
* Optional flow animation.

Provide reusable rules for:

* Value-to-state conversion.
* Thresholds.
* Visual colour changes.
* Animation parameters.
* Numeric labels.
* Equipment detail panels.
* State transitions.
* Data-quality handling.

Allow users to configure behaviours through the UI wherever practical.

If advanced behaviours require scripting, use a controlled extension mechanism rather than allowing arbitrary imported project files to execute unrestricted code.

A visual rules editor or node-based behaviour editor may be added if it meaningfully improves usability, but it must not delay delivery of the core configurable behaviour system.

---

## 9. SCADA AND INDUSTRIAL CONNECTIVITY

Industrial connectivity is a core requirement.

Design a modular connector architecture so that support for additional SCADA vendors and protocols does not require rewriting the 3D client.

Prioritise:

1. OPC UA.
2. MQTT.
3. REST APIs and WebSockets where appropriate.
4. Modbus TCP and vendor-specific integrations when justified by actual requirements.

Do not assume that every SCADA system supports every protocol.

Implement a common internal telemetry model that separates protocol-specific communication from the digital twin's business logic.

Each industrial connector should have a clear interface for:

* Connection setup and shutdown.
* Connection health.
* Tag discovery where supported.
* Tag browsing.
* Subscription management.
* Polling where necessary.
* Data type conversion.
* Value delivery.
* Timestamps.
* Quality status.
* Reconnection.
* Error reporting.
* Cancellation and resource cleanup.

Implement:

* Connection profiles.
* Endpoint configuration.
* Secure credential handling.
* Connection testing.
* Tag search and filtering.
* Tag subscriptions.
* Configurable polling.
* Bounded exponential backoff for reconnection.
* Timeouts.
* Per-tag status.
* Stale-data detection.
* Communication-loss detection.
* Engineering unit support.
* Batch tag import.
* Connector diagnostics.

A telemetry record should contain appropriate information such as:

* Stable tag identifier.
* Source connection.
* Source address, node identifier or topic.
* Data type.
* Value.
* Source timestamp where available.
* Receive timestamp.
* Quality or validity status.
* Engineering unit where applicable.
* Connection status.

Distinguish the last received value from a confirmed current value.

If communication is lost, the digital twin must not continue presenting old values as unquestionably current.

Represent bad-quality, unavailable, stale and unknown data states explicitly.

Do not claim production-ready protocol support just because a connector interface or demonstration screen exists.

A working connector must be accompanied by meaningful tests.

### Important safety requirement

The first release should focus on read-only integration with real SCADA systems.

The application must not write commands to real PLCs merely because a user clicks a 3D object.

If write functionality is ever introduced, it must be a separate, explicitly authorised capability with access controls, confirmation workflows, audit logs, safeguards and appropriate engineering review.

Never bypass existing SCADA, PLC, safety interlock or plant protection mechanisms.

The digital twin is not a replacement for the authoritative industrial control system.

---

## 10. TAG MAPPING STUDIO

Build a dedicated tag mapping interface.

An engineer should be able to:

1. Select an equipment instance.
2. Browse or search available source tags.
3. Select a tag.
4. Select the target equipment property.
5. Configure type conversion and scaling.
6. Configure engineering units.
7. Map source values to equipment states.
8. Configure visual behaviour.
9. Validate the mapping.
10. Test with simulated data.
11. Save the mapping.
12. Reuse the mapping configuration elsewhere.

Support bulk mapping using CSV or another documented format.

Include:

* Mapping previews.
* Clear validation errors.
* Duplicate mapping detection.
* Missing connection detection.
* Unsupported type detection.
* Unit mismatch warnings.
* Invalid state mapping detection.
* Unused mapping reports.
* Tag naming pattern rules.
* Reusable mapping templates.

Make it possible to map hundreds or thousands of tags without manually opening every equipment object.

Where appropriate, implement rule-based mapping suggestions using equipment names, tag naming conventions, source metadata and user-defined patterns.

Any automated mapping suggestion must be reviewable and explicitly accepted by an engineer.

Do not silently guess mappings and treat them as verified.

---

## 11. LIVE DIGITAL TWIN RUNTIME

Build a functioning runtime that connects the 3D scene to telemetry and equipment configuration.

Include:

* Interactive 3D navigation.
* Equipment selection.
* Equipment detail panels.
* Live telemetry displays.
* State visualisation.
* Alarm indicators.
* Historical trends.
* Equipment hierarchy.
* Search by name or identifier.
* Filters by type, state and alarm condition.
* Connection-health indicators.
* Last-updated timestamps.
* Full-screen mode.
* Presentation mode.
* Clear live/simulation indicators.

Make telemetry updates efficient.

Receiving a new tag value must not unnecessarily rebuild the entire 3D scene or reload all assets.

Decouple data reception from rendering. Batch or throttle UI updates appropriately while preserving important state transitions and alarms.

The runtime must clearly distinguish:

* Live values.
* Simulated values.
* Stale values.
* Invalid-quality values.
* Unavailable values.

The visual presentation should be suitable for engineering demonstrations and operational monitoring.

Do not use fake data in live mode without an explicit indication.

---

## 12. ALARM MANAGEMENT

Implement a reusable alarm subsystem.

Support:

* Configurable alarm definitions.
* Threshold-based alarms.
* State-based alarms.
* High, high-high, low and low-low conditions where applicable.
* Alarm severity.
* Activation and return-to-normal events.
* Active alarm lists.
* Alarm history.
* Acknowledgement tracking.
* Timestamps.
* Associated equipment.
* Alarm source.
* Search and filtering.
* Debounce, hysteresis and deadband where appropriate.
* Communication-loss alarms.
* Persistence and recovery.

Provide a clear visual distinction between alarms, warnings, equipment faults and communication-quality problems.

Alarm processing should be independent of the rendering loop.

Avoid losing alarm transitions because a UI update was delayed.

Document the persistence, ordering, acknowledgement and recovery behaviour.

The application must not represent itself as a certified safety system or replace a plant's existing alarm infrastructure.

---

## 13. HISTORICAL TELEMETRY AND TRENDS

Implement:

* Current-value displays.
* Time-series charts.
* Configurable time ranges.
* Multiple traces.
* Engineering units.
* Axis labels.
* Historical queries.
* Missing-data indicators.
* Time-range selection.
* CSV export where permitted.
* Configurable data retention.
* Efficient queries.
* Appropriate aggregation and downsampling for long time ranges.

Separate real-time distribution from historical storage.

Do not write every UI redraw to the database.

Design the telemetry pipeline to handle the expected volume without excessive database load.

Begin with a practical storage architecture. Introduce more complex time-series infrastructure only when requirements justify it.

Document retention, sampling, aggregation and missing-data policies.

---

## 14. WATER TREATMENT AND PUB-ORIENTED TEMPLATES

Provide reusable templates suitable for water infrastructure without assuming that all facilities share the same design.

Consider templates for:

* Water storage tanks and reservoirs.
* Pumping stations.
* Intake and transfer pipelines.
* Valves and flow meters.
* Filtration-related equipment.
* Chemical dosing equipment.
* Treatment process stages.
* Level, pressure, flow and water-quality instrumentation.

Provide configurable equipment hierarchies and process overview layouts.

Allow users to represent relationships between process stages and equipment.

Where useful, provide reusable visual components for tanks, pumps, pipelines and process flow diagrams.

All example facilities, diagrams, tag names and telemetry must be illustrative unless supplied from an authorised engineering source.

Do not embed real PUB operational configurations, credentials or sensitive site data.

Do not make unverified assumptions about plant operation, safety limits or process engineering.

The platform must remain generic enough for use in other industrial environments.

---

## 15. TEMPLATES, AUTOMATION AND CROSS-PROJECT REUSE

Make reuse a first-class capability.

Support:

* Global component templates.
* Project-specific templates.
* Reusable equipment definitions.
* Reusable animations.
* Reusable tag mapping rules.
* Configurable naming conventions.
* Bulk property editing.
* CSV metadata import/export.
* Copying components between compatible projects.
* Library versioning.
* Dependency tracking.
* Configuration comparison.
* Validation of imported projects.
* Upgrade and migration workflows.

A project template must be usable without access to the original project's database or filesystem.

Avoid coupling reusable assets to one specific project.

Make it easy to create a new project by selecting a template, importing models and configuring only the differences.

Potential future improvements include automated model change detection, tag mapping suggestions, IFC workflows, AI-assisted asset identification and predictive maintenance.

Treat these as future enhancements unless they are implemented and tested.

Do not let advanced features distract from delivering the core reusable platform.

---

## 16. USER INTERFACE AND EXPERIENCE

The software must look and behave like a professional engineering application.

Avoid a generic admin dashboard with a decorative 3D viewport.

For the editor, consider a layout with:

* Project navigation.
* Asset library.
* Scene hierarchy.
* Central 3D viewport.
* Properties inspector.
* Tag mapping panel.
* Validation and diagnostics panel.

For the runtime, consider:

* Main 3D scene.
* Facility hierarchy.
* Equipment detail panel.
* Live telemetry.
* Alarm panel.
* Historical trends.
* Connection status and operating mode.

Implement consistent typography, spacing, colours, icons and interaction patterns.

Prioritise:

* Useful navigation.
* Context-sensitive actions.
* Search and filtering.
* Clear empty states.
* Progress indicators.
* Meaningful error messages.
* Undo and redo where practical.
* Keyboard shortcuts where useful.
* Readable equipment labels.
* Accessible controls.
* Consistent visual status indicators.

All visible controls must perform their advertised functions.

Do not leave important buttons as placeholders.

If a feature is not implemented, either omit the control or clearly label it as unavailable.

Prioritise complete user workflows over the number of screens.

---

## 17. SECURITY AND INDUSTRIAL DEPLOYMENT

Assume that the software may eventually be deployed on engineering workstations or within restricted industrial environments.

Design for:

* Least-privilege access.
* Role-based access control where required.
* Secure authentication.
* Secure credential storage.
* TLS where supported.
* Certificate validation.
* Audit logging.
* Input validation.
* Safe import/export.
* Dependency management.
* Secure defaults.
* Configurable retention.
* Backup and recovery.
* Offline installation where required.
* Separation of development, simulation and production configurations.

Never hardcode passwords, tokens, certificates or production endpoints.

Never disable certificate validation just to make a connection work.

Do not assume access to the public internet.

Avoid unnecessary outbound network dependencies.

Document required ports, services, firewall rules, certificates, credentials and deployment assumptions.

Keep credentials out of project exports, logs, source control and diagnostic bundles by default.

Make it possible to install the application on a clean supported machine using documented steps.

Use Docker where it simplifies deployment, but do not require Docker for every component if that creates unnecessary difficulty in the target environment.

The final deployment strategy must reflect the chosen architecture and actual customer requirements.

---

## 18. OPERATIONS, DIAGNOSTICS AND MAINTENANCE

Implement:

* Structured logging.
* Connector diagnostics.
* Connection-state monitoring.
* Application health checks.
* Import diagnostics.
* Error reporting.
* Performance metrics where practical.
* Exportable diagnostic bundles with secrets excluded.
* Database backup and restore.
* Configuration validation.
* Troubleshooting documentation.

Avoid logging credentials or unnecessarily exposing sensitive site information.

Errors should identify the failing operation and provide actionable diagnostic information.

Handle network interruptions, malformed data, unsupported file formats, missing dependencies and database failures gracefully.

Do not let a failed optional connector crash the entire application.

---

## 19. TESTING AND QUALITY ASSURANCE

Testing is mandatory.

Do not equate code generation with successful implementation.

Use the test frameworks appropriate to the selected technologies.

Implement:

* Unit tests.
* Integration tests.
* End-to-end tests where practical.
* Import/export tests.
* Persistence tests.
* Connector tests.
* Data validation tests.
* Alarm processing tests.
* Security-sensitive workflow tests.
* Performance tests for important workloads.

Provide a simulated SCADA connector so that the application can be developed and tested without access to a real industrial plant.

The simulator should support realistic but clearly labelled scenarios such as:

* A pump starting and stopping.
* Tank levels rising and falling.
* A valve changing position.
* Sensor values crossing thresholds.
* Alarm activation and recovery.
* Communication loss.
* Stale telemetry.
* Reconnection.
* Invalid or missing data.

The simulator must not accidentally connect to real equipment or write to a production system.

For every feature, verify the complete workflow.

For example, model import is not complete just because a file picker opens. The model must be imported, displayed, selectable, editable and saved successfully.

Tag mapping is not complete just because the mapping appears in a database. It must affect the intended equipment property and behave correctly when telemetry changes.

Project persistence is not complete until the application can be restarted and the project restored.

When reporting test results, distinguish clearly between:

* Tests that passed.
* Tests that failed.
* Tests not run.
* Tests that could not be executed due to environmental limitations.
* Manual verification that was actually performed.

Never fabricate test results or claim that an application was run if it was not.

---

## 20. PERFORMANCE AND SCALABILITY

Do not make unsupported claims about capacity.

Define representative test scenarios and measure the application.

Consider:

* Number of visible assets.
* Model complexity.
* Number of connected tags.
* Tag update frequency.
* Subscription count.
* Historical query duration.
* Alarm event volume.
* Project load time.
* Memory consumption.
* Rendering responsiveness.

Use suitable optimisation techniques, such as batching, caching, instancing, level of detail, background processing, efficient database access and controlled UI refresh rates.

Avoid coupling rendering frequency to the raw frequency of incoming telemetry.

Document measured results, tested limits and known bottlenecks.

Optimise based on actual measurements rather than premature complexity.

---

## 21. DEVELOPMENT CHECKPOINTS

Develop the product through sequential checkpoints.

Every checkpoint must leave the repository in a coherent state and deliver a useful increment.

### Checkpoint 1: Application skeleton

Implement the application structure, startup, basic UI, project creation, initial 3D viewport, configuration persistence, logging and basic tests.

Acceptance criterion: Create a project, add a basic object, save it, restart the application and load it again.

### Checkpoint 2: Model import and scene editing

Implement the selected model import pipeline, scene hierarchy, object selection, transformations, property editing and persistence.

Acceptance criterion: Import a model, edit its placement, save the project, restart the application and verify the restored scene.

### Checkpoint 3: Equipment library

Implement reusable equipment definitions, instances, metadata, duplication, persistence and templates.

Acceptance criterion: Create a pump definition, place multiple instances, customise one instance and verify that the other instances remain independent.

### Checkpoint 4: Simulated telemetry and tag mapping

Implement the simulator, tag model, mapping interface, state normalisation and telemetry-driven visual behaviours.

Acceptance criterion: Simulate pump and tank telemetry, map the values and verify that the 3D equipment changes state appropriately.

### Checkpoint 5: Real SCADA connectivity

Implement and test the first real industrial connector, prioritising OPC UA where practical. Add MQTT or other connectors according to requirements.

Acceptance criterion: Connect to a dedicated test server, browse or subscribe to tags, process updates, detect connection loss and recover correctly.

### Checkpoint 6: Runtime, alarms and trends

Implement the operational runtime, equipment detail panels, alarms, alarm history, acknowledgement and historical telemetry.

Acceptance criterion: Generate controlled telemetry and alarm transitions and verify the values, events, timestamps and runtime displays.

### Checkpoint 7: Reuse and project portability

Implement project templates, import/export, bulk mapping, configuration validation, migration and library versioning.

Acceptance criterion: Create a second project from a template and verify that the equipment and behaviours are preserved while project-specific configuration remains independent.

### Checkpoint 8: Security, deployment and optimisation

Implement the appropriate security controls, installation procedures, diagnostics, backup and recovery, and performance improvements.

Acceptance criterion: Install the software in a clean supported environment, load a project, use the simulator and restore a backup.

### Checkpoint 9: Release candidate

Verify the integrated application, documentation, deployment procedures, core workflows, integration tests, security-sensitive operations and known limitations.

Deliver a release-readiness report.

These checkpoints are a roadmap, not a requirement to complete one entire checkpoint in a single response. Large checkpoints must be divided into smaller implementation increments as explained in the continuation rules.

---

## 22. MANDATORY CONTINUATION RULES

This section is extremely important.

I will not provide detailed continuation instructions after each response. My continuation message will simply be:

**continue**

Whenever I say "continue", follow these rules.

### Rule A: Inspect before proceeding

First inspect the existing repository, current files, project documentation, latest checkpoint report and any continuation notes.

If the working environment preserves the previous session's filesystem, use it.

If the conversation continues in a fresh environment, inspect the available project files and recover the latest state from the repository.

Do not assume that you remember the previous session accurately.

Do not assume that previously generated code works.

Do not overwrite working files simply because you cannot recall how they were implemented.

### Rule B: Determine the next task autonomously

Based on the actual project state, identify the highest-priority incomplete task.

Prioritise in this order:

1. Broken builds, startup failures and regressions.
2. Defects preventing the current feature from working.
3. Completion of the current implementation increment.
4. Missing acceptance tests and verification.
5. The next logical implementation increment.
6. Documentation and maintainability improvements.
7. Lower-priority enhancements.

Do not jump to an exciting new feature while a core workflow is broken.

Do not repeatedly redesign the architecture without a concrete reason.

### Rule C: Use appropriately sized implementation increments

Do not attempt to complete an entire large checkpoint in one response.

Also do not make an unnecessarily tiny change that produces little meaningful progress.

Choose a unit of work that can be implemented, verified and documented within the available context and execution budget.

A typical continuation should implement one coherent feature slice, such as:

* Completing project save/load functionality.
* Implementing model import and error handling.
* Adding the scene hierarchy and object selection.
* Implementing reusable equipment definitions.
* Connecting simulated telemetry to equipment state.
* Adding tag mapping validation.
* Implementing OPC UA subscription handling.
* Adding alarm persistence.
* Completing project export/import.

The examples are illustrative. Select the task that best fits the actual project state.

If a feature is too large, divide it into smaller steps with explicit intermediate acceptance criteria.

Prefer a working vertical slice over a large amount of disconnected code.

### Rule D: Respect context and token limits

Before starting a continuation, estimate how much work can be completed reliably in the available context.

Do not generate thousands of lines of unrelated code in one response.

Do not spend the entire response repeating architecture documentation or describing your plans.

Spend most of the available effort implementing and verifying actual software.

If the next task would exceed the available context, implement a smaller coherent part and leave the remaining work for the next "continue".

Do not start so many unfinished changes that the repository becomes difficult to recover.

Keep the work bounded enough that you can still summarise the actual changes, test results and remaining tasks.

If tools or execution time are limited, prioritise a smaller verified change over a larger unverified change.

### Rule E: Implement, test and repair

For each continuation:

1. Identify the task and acceptance criteria.
2. Inspect the relevant existing code.
3. Implement the required changes.
4. Run appropriate builds and tests.
5. Fix failures caused by the changes where feasible.
6. Check for obvious regressions.
7. Update relevant documentation.
8. Record the exact implementation state.

Do not stop after producing a plan when you have the tools and context to implement the task.

Do not claim success unless the relevant result has been verified.

If you cannot execute a test, state that limitation honestly.

### Rule F: Maintain persistent project memory

Maintain a root-level `CONTINUATION.md` file.

Update it whenever a meaningful implementation increment is completed.

It must contain:

* Product objective.
* Approved architecture and major decisions.
* Repository structure.
* Implemented modules.
* Current checkpoint.
* Current implementation increment.
* Build and test commands.
* Actual verification results.
* Known bugs.
* Unimplemented requirements.
* Current blockers.
* Important files changed.
* Exact next recommended task.

Keep the file concise enough to remain useful but sufficiently detailed for a new AI session to resume development.

Also maintain:

* `README.md` for setup and usage.
* `docs/ARCHITECTURE.md` for system architecture.
* `docs/REQUIREMENTS.md` for requirements and acceptance criteria.
* `docs/DECISIONS/` for major architecture decisions where appropriate.
* Appropriate testing and deployment documentation.

Do not create duplicate documentation files unnecessarily. If equivalent documentation already exists, update it.

### Rule G: End every continuation with a checkpoint summary

At the end of each "continue" response, provide a concise summary containing:

1. What was implemented.
2. The important files changed.
3. Commands and tests actually executed.
4. Actual test results.
5. Known defects and unverified functionality.
6. Progress against the current checkpoint.
7. The next logical task.

Update `CONTINUATION.md` before ending whenever the repository is accessible.

If the working environment cannot preserve changes between sessions, explain that limitation and provide a recovery plan rather than pretending that the project state is safely persisted.

### Rule H: Do not confuse partial completion with failure

It is acceptable for the complete product to require many continuation turns.

It is not acceptable to repeatedly restart the application, replace the architecture, abandon unfinished work or generate a new skeleton on every turn.

Preserve the existing repository and accumulate working functionality.

If a feature requires more than one continuation, clearly record what has already been completed and implement the next missing part.

The objective is continuous progress toward a functioning integrated product.

---

## 23. CODE AND REPOSITORY MANAGEMENT

Use the actual development environment available to you.

If you have filesystem and terminal access, create and modify real project files and execute commands.

If you have access to an existing repository, inspect it before changing anything.

If no repository exists, initialise one with a clean directory structure and version control where supported.

Use a modular layout appropriate to the chosen architecture. For example, the system may contain separate areas for:

* 3D editor.
* Runtime.
* Backend API.
* Core domain models.
* Equipment library.
* Industrial connectors.
* Telemetry.
* Alarm processing.
* Historical storage.
* Project management.
* Tests.
* Documentation.
* Deployment configuration.

Adapt the structure to the chosen stack rather than copying this list mechanically.

Keep the number of projects and services manageable.

Pin important dependencies and document prerequisites.

Provide practical setup instructions for the development environment.

Use migrations for persistent database schema changes where applicable.

Keep secrets out of version control.

Do not replace working files wholesale when a targeted modification is sufficient.

If you must perform a large refactor, identify the reason and add appropriate regression tests.

When a build fails, investigate the underlying error instead of blindly changing unrelated code.

---

## 24. ACCEPTANCE STANDARD FOR EVERY FEATURE

A feature is complete only when it is implemented, integrated, persisted where necessary and tested to a reasonable standard.

Use these principles:

* A visible button must perform its intended action.
* An imported model must actually appear in the scene and be usable.
* A saved project must survive application restart.
* A reusable component must be reusable across instances and projects according to its documented scope.
* A tag mapping must affect the intended equipment property.
* A telemetry update must be handled according to its value, timestamp and quality.
* A disconnected source must not appear healthy.
* An alarm must be generated and recorded according to its configuration.
* An export must produce a usable, documented project package.
* A deployment guide must correspond to the actual application.
* A connector must have real implementation and test evidence.
* A simulation must be clearly distinguished from real telemetry.

Do not mark a feature complete merely because a class, API endpoint, database table or UI screen exists.

Avoid placeholder implementations that return hardcoded success responses.

Use realistic sample projects and automated test data.

When complete integration is not yet possible, implement the smallest working increment and clearly document the missing pieces.

---

## FINAL OBJECTIVE

Deliver a functioning, maintainable, extensible Industrial Digital Twin Platform that enables our engineers to create reusable digital twins for water treatment and other industrial projects with substantially less repetitive custom development.

The platform must integrate 3D models, equipment configurations, SCADA telemetry, reusable behaviours, alarms, historical trends, project templates, testing and deployment workflows.

Prioritise engineering usefulness, maintainability, real integration, security, reliable persistence and measurable reductions in repetitive work.

Build a coherent product, not a collection of disconnected demonstrations.

Work incrementally, preserve progress across sessions, test what you implement, document what remains, and continue autonomously whenever I say "continue".

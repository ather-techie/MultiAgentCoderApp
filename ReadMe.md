# MultiAgentCoderApp

MultiAgentCoderApp is a **multi-agent code generation system** built on **.NET 10**.  
It coordinates specialized agents to analyze requirements, generate code, scaffold projects, create tests, and validate build output in a deterministic and extensible way.

The system is designed for **research, experimentation, and production-grade automation**, with strong emphasis on separation of concerns, testability, and orchestration clarity.

---

## Key Features

- Agent-based architecture with single-responsibility agents
- Central orchestrator to coordinate workflows
- Strongly-typed contracts for agent communication
- Automatic project and test scaffolding
- Build and test validation as first-class concerns
- Semantic Kernel integration for LLM interactions
- Local and remote model support

---

## Prerequisites

- .NET 10 SDK
- Git
- Optional: Local or remote LLM endpoint (OpenAI-compatible)

---

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/ather-techie/MultiAgentCoderApp.git
   ```

2. Change into the project directory:
   ```bash
   cd MultiAgentCoderApp
   ```

3. Restore and build:
   ```bash
   dotnet restore
   dotnet build
   ```

---

## Usage - RUN with command line

The console project is the main entry point. The console application runs an interactive orchestrator that prompts for a problem statement and executes the multi-agent workflow.

Provide a concise description of the requirement and the orchestrator will coordinate agents to analyze, scaffold, generate code, and validate output.

Configuration is driven via `appsettings.json`, including model selection and endpoints.

### sample with minimal parameters
macode generate code --problem="write program to add two numbers" 

macode generate full --problem="write program to add two numbers" 

macode generate full 
    --problem="write program to add two numbers" 
    --name=CalculatorApp
    --out="C:\output"
    --qa-out="C:\output"

macode generate test
    --name=GeneratedProject
    --code-file="Sum.cs"
    --proj-name=CalculatorApp.Code
    --ut-proj-name=CalculatorApp.Tests
    --out="C:\output"
    --qa-out="C:\output"

### sample with all parameters
macode generate code \
  --problem="Build a number addition service" \
  --description="Console app with clean code and unit tests" \
  --name=NumberAdder \
  --language=C# \
  --framework=net8.0 \
  --namespace=NumberAdder.Core \
  --proj-name=NumberAdder.Code \
  --ut-proj-name=NumberAdder.Tests \
  --out="C:/Generated/NumberAdder" \
  --qa-out="C:/Generated/NumberAdder" \
  --type=Console \
  --code=true \
  --scaffold=true \
  --review=true \
  --tests=true \
  --build=true \
  --failfast=true \
  --maxfix=2

  ### TODO CLI Commands 
  macode init → generates projectspec.json
  macode validate → validates spec + CLI
  macode explain → prints what agents will run
  Auto-generated --help from ProjectSpec

---

## Project Structure

```
MultiAgentCoderApp/
│
├── MultiAgentCoder.Agent*        # Agent implementations (project folder name: `MultiAgentCoder.Agent`)
├── MultiAgentCoder.Console       # Orchestrator and entry point
├── MultiAgentCoder.Contracts     # DTOs and contracts
├── MultiAgentCoder.Domain        # Domain models and enums
├── MultiAgentCoder.Infrastruture  # External integrations (note: project folder spelling matches repository)
├── MultiAgentCoder.Tests         # Unit and integration tests
└── docs                          # Documentation
```

*Some project folder names may vary slightly from the logical component name — consult the solution file or project list for exact paths.

---

## Agent Model

Agents are small, focused components that perform a single task in the code-generation lifecycle.

### Examples

- `Analysis Agent` – Interprets requirements and produces a plan
- `Project Scaffolding Agent` – Creates buildable project structures
- `Code Writer Agent` – Generates source code
- `Test Writer Agent` – Generates unit tests
- `Test Runner Agent` – Builds projects and executes tests
- `Documentation Agent` – Produces README and usage docs

Agents do **not** call each other directly.  
All execution is coordinated by the orchestrator.

---

## Orchestrator Responsibilities

- Sequence and parallelize agents
- Enforce timeouts and retries
- Aggregate diagnostics and artifacts
- Decide fail-fast vs recovery paths
- Persist final outputs

---

## Build & Test Validation

Generated output is considered successful only if:

- Projects restore successfully
- Code compiles
- Tests build and run

Failures return structured diagnostics instead of silent success.

---

## Configuration

LLM and runtime configuration is defined in `appsettings.json`:

```json
{
  "OpenAI": {
    "ModelId": "qwen2.5-7b-instruct-openvino-gpu:2",
    "Endpoint": "http://localhost:55711/v1",
    "ApiKey": ""
  }
}
```

Semantic Kernel is used to abstract model interaction and HTTP concerns.

---

## Running Tests

Run all tests using:

```bash
dotnet test
```

---

## Design Principles

- Explicit over implicit
- Agents are decision-makers, services are utilities
- Deterministic workflows
- Buildable output over raw text generation
- Strong typing for reliability

---

## Future Roadmap

- Web UI for orchestration and monitoring
- Distributed agent execution
- Capability to run Agents independently when requested
- Support for languages other than C#
- Support integrate with existing applications
- Integration with additional LLM providers
- Enhanced logging and telemetry
- Plugin architecture for custom agents
- CI/CD pipeline generation
- Improved guardrails and validation
- Expanded test coverage

---

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push and open a Pull Request

Please follow existing conventions and include tests.

---

## License

MIT License. See `LICENSE` file for details.

---

## Citation

```bibtex
@software{MultiAgentCodeApp2025,
  author = {Husain, Ather},
  title = {MultiAgentCodeApp - Multi-Agent Code Generation System},
  year = {2025},
  publisher = {GitHub},
  url = {https://github.com/ather-techie/MultiAgentCodeApp}
}
```

---

## Contact

**Ather Husain**  
Twitter: https://twitter.com/ather_techie  
Email: ather.techie@gmail.com  

Project: https://github.com/ather-techie/MultiAgentCodeApp

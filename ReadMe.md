# MultiAgentCoderApp

MultiAgentCoderApp is a **multi-agent code generation system** built on **.NET 10**.  
It coordinates specialized agents to analyze requirements, generate code, scaffold projects, create tests, and validate build output in a deterministic and extensible way.

The system is designed for **research, experimentation, and production-grade automation**, with strong emphasis on separation of concerns, testability, and orchestration clarity.

---

## Key Features

- **Two-tier agent architecture**: Role agents (DevAgent, QaAgent) coordinate core agents
- **Iterative refinement loop**: Code generation with automated review and feedback (up to 3 iterations)
- **Semantic Kernel integration** for LLM-powered code generation and review
- **Automatic project scaffolding**: Creates `.csproj` files and `Program.cs` when needed
- **Build validation**: Compiles generated code and reports errors
- **Test generation and execution**: Creates unit tests and validates them
- **CLI-first design**: Powerful command-line interface with flexible options
- **Strongly-typed contracts**: `ProjectSpec` and `WorkflowContext` for agent communication
- **Local and remote LLM support**: Works with OpenAI-compatible endpoints

---

## Prerequisites

- **.NET 10 SDK**
- **Git**
- **Local or remote LLM endpoint** (OpenAI-compatible, e.g., LM Studio, Ollama, OpenAI)

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

4. Configure your LLM endpoint in `appsettings.json`:
   ```json
   {
     "OpenAI": {
       "ModelId": "qwen2.5-coder-14b-instruct-generic-cpu:4",
       "Endpoint": "http://localhost:60035/v1",
       "ApiKey": ""
     }
   }
   ```

---

## Usage

The console application is built as **`macode`** (Multi-Agent Code) - a powerful CLI tool for AI-driven code generation.

### Commands

```bash
macode generate <code|tests|full> [options]
macode run <tests|full> [options]
```

### Quick Examples

#### Generate Code Only
```bash
macode generate code --problem="write program to add two numbers"
```

#### Generate Full Solution (Code + Tests)
```bash
macode generate full --problem="write program to add two numbers"
```

#### Generate Tests for Existing Code
```bash
macode generate tests \
  --code-file="Calculator.cs" \
  --proj-name=CalculatorApp.Code \
  --ut-proj-name=CalculatorApp.Tests \
  --out="C:\output"
```

### Comprehensive Example with All Options

```bash
macode generate full \
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
```

### CLI Options Reference

| Option | Description | Default |
|--------|-------------|---------|
| `--problem` | Problem statement (required) | - |
| `--description` | Short description for logs | - |
| `--name` | Project name | GeneratedProject |
| `--language` | Programming language | C# |
| `--framework` | Target framework | net8.0 |
| `--namespace` | Root namespace | Same as name |
| `--proj-name` | Code project name | {Name}.Code |
| `--ut-proj-name` | Unit test project name | {Name}.Tests |
| `--code-file` | Existing code file to test | - |
| `--out` | Output directory for code | Auto-generated |
| `--qa-out` | Output directory for tests | Same as --out |
| `--type` | Project type | Console |
| `--code` | Run code agent | true |
| `--scaffold` | Run scaffolding agent | true |
| `--review` | Run review agent | true |
| `--tests` | Run test agent | true |
| `--build` | Run build agent | true |
| `--failfast` | Stop on first error | true |
| `--maxfix` | Max auto-fix attempts | 2 |

### Future CLI Commands (Planned)

```bash
macode init          # Generate projectspec.json
macode validate      # Validate spec + CLI args
macode explain       # Print execution plan
macode fix           # Fix build/test errors
macode scaffold      # Scaffold project only
```

---

## Architecture

### Project Structure

```
MultiAgentCoderApp/
│
├── MultiAgentCoder.Agent/           # All agent implementations
│   ├── CoreAgents/
│   │   ├── Dev/
│   │   │   ├── CodeWriterAgent.cs          # Generates source code using LLM
│   │   │   ├── CodeReviewerAgent.cs        # Reviews code for quality
│   │   │   └── ProjectScaffoldingAgent.cs  # Creates .csproj, Program.cs
│   │   ├── Tests/
│   │   │   ├── TestWriterAgent.cs          # Generates unit tests
│   │   │   └── TestScaffoldingAgent.cs     # Creates test project structure
│   │   └── Ops/
│   │       └── BuildAgent.cs               # Compiles and builds projects
│   ├── RoleAgents/
│   │   ├── DevAgent.cs                     # Orchestrates development workflow
│   │   └── QaAgent.cs                      # Orchestrates testing workflow
│   └── Services/
│       ├── FileService.cs                  # File I/O operations
│       └── ProjectService.cs               # Project management utilities
│
├── MultiAgentCoder.Console/         # CLI and orchestration
│   ├── Cli/
│   │   ├── CliRouter.cs                    # Routes CLI commands
│   │   └── Commands/
│   │       ├── GenerateCommand.cs          # Handles 'generate' command
│   │       └── RunCommand.cs               # Handles 'run' command
│   ├── Orchestration/
│   │   └── HybridOrchestrator.cs          # Coordinates DevAgent + QaAgent
│   ├── Prompts/                            # LLM prompt templates
│   │   ├── Coder.txt
│   │   ├── Reviewer.txt
│   │   ├── Testwriter.txt
│   │   └── UnitTestReviewer.txt
│   └── Program.cs                          # Entry point
│
├── MultiAgentCoder.Contracts/       # DTOs and contracts
├── MultiAgentCoder.Domain/          # Domain models
│   ├── Models/
│   │   ├── ProjectSpec.cs                  # Configuration contract
│   │   ├── WorkflowContext.cs              # Workflow state
│   │   ├── CodeArtifact.cs                 # Code representation
│   │   └── Results/                        # Result types
│   └── Enums/
│       ├── WorkflowStage.cs               # Workflow progression
│       ├── CodeType.cs                     # Source/Test distinction
│       └── ProjectType.cs                  # Console/Library/etc
│
├── MultiAgentCoder.Infrastructure/  # External integrations
│   └── Validator/
│       └── CSharpGuardrailValidator.cs    # Code quality validation
│
└── MultiAgentCoder.Tests/           # Unit and integration tests
```

### Agent Architecture

The system uses a **two-tier agent model**:

#### 1. Role Agents (High-Level Coordinators)
- **DevAgent**: Manages development workflow (steps 1-6)
- **QaAgent**: Manages testing workflow (steps 7-11)

#### 2. Core Agents (Specialized Workers)
- **CodeWriterAgent**: LLM-powered code generation
- **CodeReviewerAgent**: LLM-powered code review with structured feedback
- **TestWriterAgent**: LLM-powered test generation
- **ProjectScaffoldingAgent**: Creates `.csproj` and `Program.cs` files
- **TestScaffoldingAgent**: Creates test project structure
- **BuildAgent**: Invokes `dotnet build` and parses results

### Communication Flow

```
CLI → CliRouter → GenerateCommand → HybridOrchestrator
                                         ↓
                  ┌─────────────────────────────────────┐
                  │                                     │
              DevAgent                              QaAgent
                  │                                     │
      ┌───────────┼─────────────┐          ┌──────────┼─────────┐
      ↓           ↓             ↓          ↓          ↓         ↓
CodeWriter  CodeReviewer  Scaffolding  TestWriter  Reviewer  TestScaffolding
   Agent       Agent        Agent        Agent      Agent      Agent
      ↓           ↓             ↓          ↓          ↓         ↓
              BuildAgent ──────────────────────────────────────┘
```

---

## Workflow Execution

### Development Workflow (DevAgent)

The DevAgent executes the following stages:

1. **Code Generation Loop** (up to 3 iterations):
   - Generate code using `CodeWriterAgent` with problem statement + feedback
   - Review code using `CodeReviewerAgent`
   - If approved, break; otherwise, iterate with feedback
   - If critical issues found, fail workflow

2. **File Persistence**:
   - Save generated code to disk
   - Determine working directory

3. **Project Scaffolding**:
   - Ensure `.csproj` exists (create if missing)
   - Ensure `Program.cs` exists (create if missing for Console apps)
   - Save all supporting files

4. **Build Validation**:
   - Compile project using `BuildAgent`
   - Parse build errors
   - If build fails, return failure with error details

### Testing Workflow (QaAgent)

The QaAgent executes the following stages:

7. **Test Generation Loop** (up to 3 iterations):
   - Load source code if needed
   - Generate unit tests using `TestWriterAgent`
   - Review tests using `CodeReviewerAgent` (with source code context)
   - If approved, break; otherwise, iterate with feedback
   - If critical issues found, fail workflow

8. **Test File Persistence**:
   - Save unit test code to disk

9. **Test Project Scaffolding**:
   - Create test project `.csproj` with NuGet references
   - Add project reference to source code project

10. **Test Build Validation**:
    - Compile test project using `BuildAgent`
    - Parse build errors

11. **Test Execution**:
    - Run tests using `dotnet test`
    - Collect results and coverage

---

## Key Design Patterns

### ProjectSpec - The Central Contract

`ProjectSpec` is the **configuration contract** that flows through the entire system. It defines:
- **What**: Problem statement and description
- **How**: Target framework, language, project type
- **Where**: Output directories for code and tests
- **Which**: Which agents should run (execution flags)
- **Constraints**: Guardrails and build options

This eliminates ambiguity and enables flexible workflows.

### Iterative Refinement with Feedback

Both code and test generation use a **3-iteration refinement loop**:
1. Generate artifact
2. Review artifact
3. If not approved, regenerate with feedback
4. Repeat up to 3 times

This dramatically improves code quality through LLM self-correction.

### Code Artifacts

All generated code is wrapped in `CodeArtifact` or `UnitTestCodeArtifacts`:
- Tracks content, revision, timestamps
- Stores feedback history
- Maintains working directory path
- Records build output

### Workflow Stages

`WorkflowStage` enum tracks progression:
- `Initialized` → `CodeGenerated` → `CodeReviewed` → `SupportFileCreated` → `BuildSucceeded`/`BuildFailed`

This enables observability, diagnostics, and recovery.

---

## Build & Test Validation

Generated output is considered successful **only if**:

1. ✅ Code is syntactically valid
2. ✅ Project structure is complete
3. ✅ `dotnet build` succeeds
4. ✅ Unit tests compile
5. ✅ Tests execute successfully

Failures return **structured diagnostics** with compiler errors, not silent success.

---

## Configuration

### appsettings.json

```json
{
  "OpenAI": {
    "ModelId": "qwen2.5-coder-14b-instruct-generic-cpu:4",
    "Endpoint": "http://localhost:60035/v1",
    "ApiKey": ""
  }
}
```

- **ModelId**: The LLM model to use (supports local models via LM Studio, Ollama)
- **Endpoint**: OpenAI-compatible API endpoint
- **ApiKey**: API key (leave empty for local models)

### Supported LLM Providers

- **Local**: LM Studio, Ollama, LocalAI
- **Remote**: OpenAI, Azure OpenAI, Anthropic (via OpenAI proxy)
- **Requirement**: Must support OpenAI-compatible `/v1/chat/completions` endpoint

---

## Prompt Engineering

The system uses structured prompts stored in `Prompts/`:
- `Coder.txt`: Code generation with best practices
- `Reviewer.txt`: Code review with JSON-structured feedback
- `Testwriter.txt`: Unit test generation with xUnit
- `UnitTestReviewer.txt`: Test review considering source code

Prompts are designed for **deterministic, parseable output** with clear instructions.

---

## Running Tests

```bash
dotnet test
```

Tests are located in `MultiAgentCoder.Tests/` and cover:
- Agent behavior
- Workflow orchestration
- File service operations
- CLI routing logic

---

## Design Principles

1. **Explicit over implicit**: All behavior is configured via `ProjectSpec`
2. **Agents are decision-makers, services are utilities**: Clear separation of concerns
3. **Deterministic workflows**: Predictable execution order
4. **Buildable output over raw text generation**: Quality over quantity
5. **Strong typing for reliability**: Compile-time safety
6. **Iterative refinement**: Quality through feedback loops
7. **Role agents coordinate, core agents execute**: Two-tier architecture

---

## Advantages of This Architecture

- ✅ **Testability**: Each agent can be unit tested independently
- ✅ **Extensibility**: Add new agents without modifying orchestrator
- ✅ **Observability**: Track workflow stage progression
- ✅ **Recovery**: Structured error handling with diagnostics
- ✅ **Flexibility**: Enable/disable agents via CLI flags
- ✅ **Quality**: Iterative refinement with LLM feedback
- ✅ **Reliability**: Build validation ensures compilable output

---

## Future Roadmap

- [ ] **Web UI** for orchestration and monitoring
- [ ] **Distributed agent execution** for scalability
- [ ] **Independent agent execution** (run single agents on demand)
- [ ] **Multi-language support** (Python, JavaScript, Java)
- [ ] **Integration with existing codebases** (modify, not just generate)
- [ ] **Additional LLM providers** (Anthropic, Cohere, Google)
- [ ] **Plugin architecture** for custom agents
- [ ] **CI/CD pipeline generation** (GitHub Actions, Azure Pipelines)
- [ ] **Enhanced guardrails** (security scanning, performance analysis)
- [ ] **Expanded test coverage** (integration tests, E2E tests)
- [ ] **Documentation generation** (Markdown, API docs)
- [ ] **Code optimization agent** (refactoring, performance)
- [ ] **Dependency management agent** (NuGet package suggestions)

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-agent`)
3. Commit your changes (`git commit -m 'Add amazing agent'`)
4. Push to the branch (`git push origin feature/amazing-agent`)
5. Open a Pull Request

Please follow existing conventions and include tests for new agents.

---

## License

MIT License. See `LICENSE` file for details.

---

## Research & Citation

This project demonstrates:
- Multi-agent coordination patterns
- LLM-driven code generation with quality gates
- Iterative refinement through automated feedback
- Separation of concerns in AI systems

If you use this in academic work, please cite:

```bibtex
@software{MultiAgentCodeApp2025,
  author = {Husain, Ather},
  title = {MultiAgentCoderApp - Multi-Agent Code Generation System},
  year = {2025},
  publisher = {GitHub},
  url = {https://github.com/ather-techie/MultiAgentCodeApp}
}
```

---

## Contact

**Ather Husain**  
Twitter: [@ather_techie](https://twitter.com/ather_techie)  
Email: ather.techie@gmail.com  
Project: [MultiAgentCoderApp on GitHub](https://github.com/ather-techie/MultiAgentCoderApp)

---

## Acknowledgments

Built with:
- [.NET 10](https://dotnet.microsoft.com/)
- [Semantic Kernel](https://github.com/microsoft/semantic-kernel)
- [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/)

---

**⭐ Star this repo if you find it useful!**

# AppSage MCP Server
This is the MCP server implementation for the AppSage project, designed to handle HTTP transports.  
We will use ModelContextProtocol libraries to facilitate communication between the MCP client and server.
Capability implementations should be in the folder `AppSage.MCPServer/Capability'

Tools: Active, callable functions.
Prompts: Prebuilt language templates.
Resources: Passive, queryable data.

- AppSage.MCPServer/Capability/Tools contains the MCP tools capabilities.
  - Definition: Functions or methods that the client can call.
  - Use case: Provide structured, callable actions like running a query, fetching data, or performing a computation.
  - Example: A “SearchDatabase” tool that takes parameters (query, filters) and returns results.

- AppSage.MCPServer/Capability/Prompts contains the MCP prompts capabilities.
  - Definition: Predefined natural language templates that the client can fill in and send back to the model.
  - Use case: Standardize common requests or guide the model’s behavior with structured context.
  - Example: A “SummarizeText” prompt where the template is “Summarize the following text in 3 bullet points: {{text}}”.

- AppSage.MCPServer/Capability/Resources contains the MCP resource capabilities.
  - Definition: Read-only data sources that the client can query, navigate, or stream from.
  - Use case: Expose external content in a structured way without the need for explicit tool calls.
  - Example: A “KnowledgeBase” resource that allows the model to retrieve passages from documents or a file system.

The description of the capabilities are defined using attributes. However, if there is a large description,we will use a separate markdown file for the description.
They are kept in the `AppSage.MCPServer/CapabilityGuide` folder. If a capability has a description file, it will be automatically linked to the capability. If not the default attribute description will be used.



## Testing the Setup


### Test HTTP Transport:
1. Server runs on http://localhost:5000/mcp
2. All logging visible in stderr and log files
3. HTTP requests work normally

## Log Files:
- Location: Configured in appsettings.json
- Pattern: `AppSageMCPServer-{date}.log`
- Retention: 30 days
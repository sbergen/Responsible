namespace ResponsibleGherkin;

public record GenerationContext(
	string Namespace,
	string BaseClass,
	IndentInfo IndentInfo,
	string ExecutorName = "Executor");

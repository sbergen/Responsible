namespace ResponsibleGherkin;

public record GenerationContext(
	string Namespace,
	string BaseClass,
	int IndentAmount = 1,
	char IndentChar ='\t',
	string ExecutorName = "Executor");

namespace ResponsibleGherkin;

public record BackgroundFlavor(
	string SetupMethodName,
	string? SetupAttribute,
	string? Interface = null,
	string? TearDownMethodName = null);

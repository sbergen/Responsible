Feature: Unsupported keywords
  Scenario Outline: A scenario containing unsupported keywords should abort code generation
    Given a scenario with an unsupported <keyword>
    Then an error should be raised

    Examples:
    | keyword |
    | Foo     |

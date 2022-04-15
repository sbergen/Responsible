Feature: Unsupported keywords
  Scenario: A scenario containing unsupported keywords should abort code generation
    Given a scenario with unsupported keywords
    * The star is not supported
    When code is generated
    Then an error should be raised

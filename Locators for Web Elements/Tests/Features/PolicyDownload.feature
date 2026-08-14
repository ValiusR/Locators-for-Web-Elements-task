Feature: Policy Download
  As a website user
  I want to download a policy PDF from the footer
  So that I can verify the file is actually downloaded

Scenario Outline: Validate policy PDF download
    Given I am on the EPAM home page for policy download
    When I download the policy file containing "<partialFileName>"
    Then the downloaded file containing "<partialFileName>" should exist

Examples:
    | partialFileName |
    | Code-Of-Conduct |

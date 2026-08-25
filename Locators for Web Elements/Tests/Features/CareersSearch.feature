Feature: Careers Search
  As a job seeker
  I want to search EPAM careers by keyword and country
  So that I can verify job details include the searched skill

Scenario Outline: Validate position search opens matching job details
    Given I am on the EPAM home page
    When I search jobs for keyword "<keyword>" in country "<country>"
    And I open the last job card from the search results
    Then the opened job details should contain "<keyword>"

Examples:
    | keyword    | country       |
    | JavaScript | United States |
    | Java       | Lithuania     |

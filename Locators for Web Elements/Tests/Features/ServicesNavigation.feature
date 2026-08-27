Feature: Services Navigation
  As a user
  I want to open a specific EPAM service category from the Services menu
  So that I can validate the page title and related expertise section

Scenario Outline: Validate navigation to a services category
    Given I am on the EPAM home page
    When I hover over the Services menu
    And I select the "<category>" service category from the dropdown
    Then the page title should contain "<category>"
    And the "Our Related Expertise" section should be displayed

    Examples:
        | category         |
        | Generative AI    |
        | Responsible AI   |

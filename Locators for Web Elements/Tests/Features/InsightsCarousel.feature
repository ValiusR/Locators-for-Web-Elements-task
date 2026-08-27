Feature: Insights Carousel
  As a website user
  I want to open a featured carousel article on Insights
  So that I can validate title consistency between card and details page

Scenario Outline: Validate carousel article title matches details page title
    Given I am on the EPAM home page
    When I open Insights and swipe the featured carousel <swipeCount> times
    And I open the selected featured article details
    Then the opened article title should match the selected carousel article title

    Examples:
        | swipeCount |
        | 2          |
        | 3          |

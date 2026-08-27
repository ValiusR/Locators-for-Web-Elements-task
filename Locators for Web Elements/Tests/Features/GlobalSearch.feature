# Feature: Global Search
#   As a website user
#   I want to search globally by keyword
#   So that all displayed results match my search intent
#
# Scenario Outline: Validate global search results contain keyword
#     Given I am on the EPAM home page
#     When I run a global search for "<keyword>"
#     Then all global search results should contain "<keyword>"
#
#     Examples:
#         | keyword    |
#         | BLOCKCHAIN |
#         | Cloud      |
#         | Automation |

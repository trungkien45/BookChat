# Redesign ViewBook & Update DB Model

This plan outlines the steps to add the `ReadingPage` property to the `Book` model and redesign the `ViewBook` screen to include WebViews for ChatGPT and Gemini.

## User Review Required

> [!IMPORTANT]
> **UI Layout Choice**: For integrating ChatGPT and Gemini, we have a few layout options. Please review the proposed approach below:
> 
> **Proposed UI**: A **Tabbed Interface** inside the `ViewBook` page.
> - We will add a Tab Bar (row of buttons) at the top or bottom with 3 tabs: **PDF Reader**, **ChatGPT**, and **Gemini**.
> - Clicking a tab will switch the visible WebView.
> - This works perfectly on both Mobile (Android) and Desktop (Windows) without cramping the screen.
> 
> *Alternative*: A **Split Screen** (PDF on the left, AI chat on the right). This is great for Desktop but terrible for Mobile due to small screen width. If you prefer a responsive approach (Split on Windows, Tabs on Android), let me know!

## Proposed Changes

### 1. UI / ViewBook Redesign

#### [MODIFY] [ViewBook.xaml](file:///c:/Users/KIEN/Desktop/Project/BookChat/BookChat/ViewBook.xaml)
- Change the root layout to a `Grid`.
- Add a Tab Bar (e.g., a horizontal `StackLayout` or `Grid` with 3 `Button`s: "PDF", "ChatGPT", "Gemini").
- Add 3 WebViews occupying the same space:
  - `PdfViewer`: The existing PDF WebView.
  - `ChatGptWebView`: `<WebView Source="https://chatgpt.com/" IsVisible="False" />`
  - `GeminiWebView`: `<WebView Source="https://gemini.google.com/" IsVisible="False" />`

#### [MODIFY] [ViewBook.xaml.cs](file:///c:/Users/KIEN/Desktop/Project/BookChat/BookChat/ViewBook.xaml.cs)
- Add event handlers for the Tab buttons to toggle the `IsVisible` property of the three WebViews.
- When "ChatGPT" is clicked, `PdfViewer` and `GeminiWebView` are hidden, and `ChatGptWebView` is shown.

## Verification Plan

1. Verify the project builds successfully.
2. The user will deploy to Android/Windows and verify:
   - The Tab Bar switches correctly between the PDF, ChatGPT, and Gemini.
   - The AI websites load successfully within the WebViews.
   - The configured SQLite book repository continues to persist `ReadingPage`.

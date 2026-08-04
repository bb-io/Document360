# Blackbird.io Document360

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Document360 is a knowledge base platform for building and publishing product documentation, help centers and internal knowledge bases. This app lets you find articles that need translating, pull their content out as files, write translations back as new drafts, and publish them — so a localized knowledge base can be kept in step with its source without anyone copying content by hand.

## Before setting up

Before you can connect you need:

- A Document360 account with access to the knowledge base portal.
- An **API token** for the project you want to automate. In the Document360 portal, go to **Settings > Knowledge base portal > API tokens** and create a token. A token is tied to a single project, so create one token per project you want to automate.
- The **API hub URL** for your region. `https://apihub.document360.io` is the default; knowledge bases hosted in the United States use `https://apihub.us.document360.io`. A token used against the wrong region is rejected as invalid.

The team account you pick as an author has to have signed in to Document360 at least once. Accounts that were invited but never accepted are returned by the API but rejected as article authors, with the message *The user with id '…' does not exist in your project*. If you see that, choose another team account.

## Connecting

1. Navigate to Apps and search for **Document360**.
2. Click **Add connection**.
3. Name your connection for future reference, for example "Product knowledge base".
4. Fill in **API hub URL** — pick your region from the list, or type your own hub address.
5. Fill in **API token** with the token you created in the Document360 portal.
6. Click **Connect**.

## Actions

Every action that works with a single article asks for a **Workspace ID** as well as an **Article ID**. Document360 calls a workspace a project version; it scopes the article, category and locale lists so you can pick values instead of typing identifiers.

### Articles

- **Create draft article** — Creates a new draft article in a category, with its body taken from a file. Markdown files are stored as markdown so a later download returns markdown again; anything else is stored in the project's default editor format. Returns the new Article ID, its public address and the version numbers.
- **Update article** — Writes new content and metadata to one article translation and leaves it as an editable draft. This is the localization return path: the first time it runs against a language that has never been translated, it creates that translation. Content can only be written to a draft, so when the newest version is published, unpublished or in review, the action copies it into a new draft first and reports that with **Was copied to new draft**. Supply either **File** or **Content**, not both. Anything you leave empty is preserved, including the category, so the source structure survives the round trip.
- **Publish article** — Puts an article translation live and returns its public address. Leave **Version number** empty to publish the newest version. Publishing a version that is already live succeeds and reports **Was already live**, so re-running a workflow is safe.
- **Download article** — Exports the translatable fields of an article translation — title, body, description, SEO title and meta description — as one HTML file, ready to be translated. See **Translating an article** below. Leave **Resolve merge codes** off for content you intend to write back, so snippets, variables and glossary codes survive.
- **Upload article** — Writes a translated file back onto an article translation, leaving it as an editable draft. Accepts the HTML file from **Download article** or an XLIFF file produced from it by a translation app. Only the fields that actually changed are written, and **Updated fields** tells you which those were.
- **Get article info** — All details of one article translation in a single step: title, address, status, version numbers, category, tags, custom fields and — through **Available locales** — which other languages the article exists in and how far each translation has come. Use this to decide which target languages an article still needs.
- **Search articles** — Finds the articles of a workspace. Filters by locale, publication status, visibility, translation status, hidden state and last-changed date. Paging is handled for you, so a workspace with hundreds of articles comes back as one list. **Translation status** needs a locale, because it asks Document360 which articles are in that state for that language.

### Workspaces

- **Search workspaces** — The workspaces of the project, each with the languages it supports.
- **Search workspace locales** — The languages enabled for one workspace, and which of them is its source language.

### Custom fields

- **Search custom fields** — The project's custom fields, with their types and the exact option IDs to send when filling one in through **Update article**.

### Advanced settings

These inputs are optional and can be left empty:

| Action | Optional inputs |
| --- | --- |
| Create draft article | Editor format, Slug, Order |
| Update article | Title, File, Content, Translation status, Hidden, Category ID, Order, Description, Tags, Custom field IDs, Custom field values, SEO title, Meta description, User ID |
| Publish article | Version number, Publish message |
| Download article | Use published version, Version number, Resolve merge codes, File name |
| Upload article | Article ID, User ID |
| Get article info | Use published version |
| Search articles | Locale, Publication status, Visibility, Translation status, Hidden, Modified after, Max results |

To write custom fields with **Update article**, list the fields under **Custom field IDs** and their values under **Custom field values** in the same order. For a dropdown field the value is the option ID, which **Search custom fields** and **Get article info** both return.

## Translating an article

**Download article** and **Upload article** are two halves of one contract:

```
Download article  →  translate (any translation app)  →  Upload article
```

The exported file describes itself. Alongside the text to translate it carries the article's identity, which field each piece of text belongs to, and the untranslated original. That is what lets **Upload article** rebuild the article exactly as it was and change only what the translator actually changed:

- Downloading and uploading again without editing anything writes nothing back and creates no new version.
- A markdown body is exported one line at a time, so a heading, a paragraph and a list item are separate segments. Blank lines and indentation are restored from the original, so the markdown structure survives translation intact.
- An advanced WYSIWYG body is exported as HTML, so the markup travels with the text.
- The file name ends in the locale (`My article_fr.html`), which is how source and target files pair up.

Because the file follows the Blackbird content conventions, any translation app in a workflow can sit between the two steps, and the file **Upload article** returns is stamped with where the content now lives, ready for a following step to record it.

Tags and custom fields are deliberately not part of the file: they are taxonomy rather than prose. Use **Update article** for those.

## Events

Document360 has no webhooks for article changes, so both events poll the workspace on an interval you choose. Neither event fires on its first run: that run only records the current state so the next one has something to compare against.

- **On articles created** — Fires for articles created since the previous check. Filter by locale, publication status, tags and visibility.
- **On articles published** — Fires for articles that went live, or that had a newer version put live, since the previous check. Filter by locale, tags and visibility. This event compares the live version number of every article rather than the article status, because an article that is live and also has a newer draft still reports its status as a draft. It fulfils the standard Blackbird content event blueprint, so a translation workflow built on another CMS can be pointed at Document360 without being rebuilt: every article it outputs carries the article ID the blueprint's download step expects.

Filtering by tags costs one extra request per changed article, since tags are not part of the article list Document360 returns.

## Known limitations

- Tags cannot be used as a filter on **Search articles**. Document360 does not return tags in its article list, so filtering a workspace of several hundred articles by tag would need one request per article. Tag filters are available on the events, where only the handful of changed articles is inspected.
- There is no category filter on **Search articles** for the same reason: the article list carries no category.
- Article bodies are text, not binary. **Download article** builds the file from the article's content, and images inside the body stay as absolute Document360 addresses.
- Document360 allows roughly 100 requests per window. Throttled requests are backed off and retried automatically, so a workflow over a few dozen articles rides out the limit; a workflow over several hundred articles will simply run slower as it waits.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->

# SPA Framework
This is a framework for single page applications using an ASP.NET Core backend and VueJS frontend.

## Tools Used in this Project
* Back-End
  * ASP.NET 5.0
  * Entity Framework Core
  * AutoMapper
  * FluentValidation
  * Authentication with JWT
  * OpenAPI 3 Docs with Swagger (via Swashbuckle)
  * Serilog
  * Azure Web Jobs
  * SignalR
  * NodaTime
* Front-End
  * VueJS 2.x
  * Vue CLI 3
  * Vue Router
  * Vuex
  * Webpack
  * Bootstrap 4.x and BootstrapVue 2.x
  * SignalR

# Getting Started

## Dev Cert
To install the dev cert:

```dotnet dev-certs https --trust```

## VS Code
* Open the root folder in VS Code
* To run the solution, in the VS Code terminal, type: ```dotnet run --project SpaFramework.Web```
* The site will be at:
  * http://localhost:5000
  * https://localhost:5001
* HMR (Hot Module Replacement) will run for VueJS. So as you edit Vue files, you'll see them reflected in the browser without refreshing.

## Visual Studio 2019
* Open the solution file
* To run the solution, press F5
* The site will be at:
  * http://localhost:5010
  * https://localhost:5011 -- I haven't actually been able to get this to work.
* HMR (Hot Module Replacement) will run for VueJS. So as you edit Vue files, you'll see them reflected in the browser without refreshing.

## Manual
* From a PowerShell prompt, use:
  * Run-Web.ps1 to run the web app (the site will be at https://localhost:5001)
  * Run-Worker.ps1 to run the worker app

# Common Tasks

## Adding Generic Domain Models
* Adding the data model
  * Add data model class (in App\Models\Data)
    * Include a TrackedChange class as well, if needed
  * Add data model DbSet to EF schema (in App\DAL\ApplicationDbContext)
  * Add validator (in App\Models\Data)
  * Add validator singleton (in App\ServiceBuilder)
  * Add service (in App\Services\Data)
  * Add transient service reference for service (in App\ServiceBuilder)
  * Add migration (see "Entity Framework Core Commands" below)
* Adding API access
  * Add DTO model class (in DTO)
  * Add mapping relationship from data model to DTO model (in Web\Mapping\DataModelsProfile)
  * Add API controller (in Web\Controllers\Data)

## Adding Vue Components
* Create the .vue file (in ClientApp\src\components)
* Update the "name" value in the .vue file
* Add the "import" to ClientApp\src\main.js
* Add a route, if needed, to ClientApp\src\main.js
* Add a Vue.Component call, if needed, to ClientApp\src\main.js

## Adding Vue CRUD Controllers for a new Domain Model
* Create a folder in ClientApp\src\components
* Copy boilerplate files (List, Fields, Add, Edit, Delete) from another controller
* Add nav items, if needed, to ClientApp\src\App.Vue
* Add imports for the files to ClientApp\src\main.js
* Register the fields component in ClientApp\src\main.js
* Add routing rules to ClientApp\src\main.js

## Entity Framework Core Commands
* Add EF migrations with:
  * cd SpaFramework.App
  * dotnet ef migrations add Initial --context ApplicationDbContext --startup-project ..\SpaFramework.Web
  * If you're modifying views, be sure to add a call to:
    * migrationBuilder.BuildInitialViews();
    * Remove calls to BuildInitialViews from the most recent migration with that call
* Update database with:
  * dotnet ef database update --context ApplicationDbContext --startup-project ..\SpaFramework.Web
  * If that fails (timeout, etc.), run this to get the SQL script:
    * dotnet ef migrations script PreviousMigrationName --idempotent --context ApplicationDbContext --startup-project ..\SpaFramework.Web

# Notes
When running Vue CLI commands, be in the "ClientApp" folder:
```npm install```

When running .NET stuff, be in the root folder:
```dotnet run```

## Hosting
* To host on IIS, you need the .NET Core 5 Hosting Bundle for Windows.

## Useful NPM Commands (run in ClientApp)
* ```npx npm-check``` - finds unused packages and packages needing upgrades
* ```npm audit``` - shows packages with security updates
* ```npm audit | Select-String -Pattern "(High | Critical)" -Context 0,10``` - show only high and critical updates
* ```npm outdated``` - shows packages that can be upgraded/updated
* ```npm show @microsoft/signalr``` - shows available versions of a package
* ```npm list @microsoft/signalr``` - see the installed version of a package

# General Architecture
* There's a back-end/ASP.NET Core application that provides an API.
  * Entity Framework is used for data access.
  * All data access occurs through services. The services use generics extensively, performing out-of-the-box support for basic CRUD operations and a logical place for adding additional application/business logic.
* There's a .NET worker application that handles background processing.
  * Background work can be scheduled by using Schedulers.
  * Individual tasks can be queued using WorkItems.
  * By default, the worker uses Azure Storage. However, you could use something like Hangfire instead.
* There's a front-end VueJS application the provides the main UX and interacts with the API.

# Back-End / ASP.NET Core Notes

## Authentication and Open API
* The entire API is documented with an Open API spec.
  * This spec is generated automatically based on the managed code.
  * Triple slash documentation/comments on data models is automatically copied to corresponding DTOs using the DtoMappingSchemaFilter.
  * Swagger is included for viewing the spec: https://localhost:5001/swagger/index.html
* Both JWT and HTTP Basic Authentication are supported for authentication. The Open API spec contains more details about authenticating.

## Entity Change Tracking
* Entities can have a corresponding `TrackedChange` class associated with them. This performs three types of change tracking:
* Every entity has a boolean `Deleted` field that dictates whether or not the entity is active. An inactive entity, for most purposes, should be treated as a soft delete. Another way to look at it is that an inactive entity only exists for analytical, not operational, purposes. It's kept in data so we don't have to worry about any foreign key constraints breaking when deleting the entity.
* The `LastModification` field is updated on every modification of the field.
* Changes, including a serialized before and after snapshot of the object, are stored on every change.

## Worker and Work Items
* All background processing should occur in the Worker project.
* The Worker project is designed to run as an Azure Web Job. It can also be run from the command-line for debugging and development, and could potentially run as a long-running command-line application in another environment.
* There are two types of background processing:
  * Schedulers
    * Schedulers run on a regular basis.
    * They use the Web Jobs TimerTrigger to schedule their runs. The Web Jobs library enforces singleton use of these -- in other words, even if there are multiple instances running, only one will trigger a given task.
    * Schedulers shouldn't really perform work -- they should just enqueue a Work Item and complete. The scheduler is merely a mechanism for triggering the process. Put differently, the scheduler should be able to be replaced by some other scheduling component without causing issues.
  * Work Item Processors
    * Work item processors are triggered by a work item queue.
    * Use the IWorkItemService<T> to enqueue work item messages.
* There are two interfaces for working with work items.
  * IWorkItemService is designed for internal uses where user authentication/authorization isn't required. For instance, application/business logic that needs to enqueue a work item or the work item processors. Note that this instances of this interface are intended to be singletons.
  * ISecureWorkItemService is designed for external use where user authentication/authorization is required. This is what web services use; in general, most work items aren't triggered externally and don't use this interface.
* Work items that fail are automatically requeued and tried again. Be careful if there are operations in the work item processor that aren't full atomic. For instance, a work item processor that sends an email and then records that an email is sent could fail when recording the send. This would cause the entire work item to be re-run, which in turn would cause the email to be resent, since the failure in processing occurs after that point.

## Jobs
* A job is an operation that's run in the backround (by the worker) for multiple items.
* A job has an expected number of items that it should process, and tracks the success or failure of each individual item.
* Generally, each item processed by a job gets its own work item.
* Each item can, optionally, log a result or error.

## Seed Data
* ApplicationDbContext has code that seeds some test data -- a handful of users, content pages, and domain-specific data (clients and projects). In a real-world scenario, you probably don't want this seeded -- it'll reset this data to match what's in managed code every time the app starts.

## SignalR
* The framework supports SignalR for real-time communications. Look at NotificationHub and NotificationClient for an example.
* On the front-end, App.vue manages the SignalR connection.

## Views
* You can create your own SQL views and use them as you would any other entity (except that they're read-only).
* ClientStats is an example of this.
* There's a view (build in BuildViews.BuildInitialViews) that is created automatically as part of EF migrations (assuming you call modelBuilder.BuildInitialViews in your migration).
* You can create relationships/navigation properties on views the same as any other entity.

## Optimistic Concurrency
Data entities use optimistic concurrency. There are properties for ConcurrencyTimestamp and ConcurrencyCheck that enforce this. If there's a concurrency conflict (two people editing the same entity), the API returns an HTTP 409 when trying to save the entity, indicating that the data has changed.

# API Notes
The following notes on the API also appear in the Open API spec (and are defined in Startup.cs).

## Authentication
Most of these API endpoints are only available to authenticated users. To authenticate, make a call to `GET /api/account/login`. Assuming valid credentials are provided, a JWT token will be returned. This token should be included in the `Authorization` header as a bearer token on subsequent requests.

Basic HTTP Authentication is also supported, using the standard pattern of `username:password`.

Within Swagger UI, you can click the 'Authorize' button to use either a JWT token or basic authentication on requests.

## Including Related Data
The `includes` parameter on many of these calls lets you specify related objects to include in the results. The parameter is a comma-delimited list of related objects; the objects use dotted notation. For instance, to include `schedule` items when calling `/api/donors`, set `includes` to `schedules`. If you also wanted to include `itemTypes`, which is a property of `schedule`, you'd set `includes` to `schedule,schedule.itemTypes`.

## Filtering Results
Results can be filtered with the `filter` parameter. You can use basic conditional operations, parentheses for grouping, null checks, and operators like AND and OR. Here are some examples (using the `/api/donor` endpoint):
* `city=""PORTLAND""`
* `city=""Portland"" or state=""OR""`
* `phoneNumber1 != null`
* `phoneNumber1 = null`
* `(city=""Portland"" or state=""OR"") and phoneNumber1 != null`
* `id = 10000`

There are also a handful of functions you can use, including `StartsWith`, `EndsWith`, and `Contains`. Note that, especially across large datasets or non-indexed columns, these can perform poorly.
* `city.Contains(""Port"")`
* `city.Contains(""Port"") and !city.EndsWith(""land"")`

Note that not all fields can be filtered.

By default, filter values follow the database collation, so they're case insensitive.

Double quotes should be used around string literals.

Some lambda expressions can also be used. For instance, if there's a many-to-many relationship from `post` to `author` (where `post` has an `authors` navigation property), a filter  like this against `post` would return all posts by the author John Doe: `authors.Any(x => x.FirstName = ""John"" and x.LastName = ""Doe"")`

## Ordering Results
Results can be ordered with the `order` parameter. This is a comma-separted list of fields to sort on. Each field can have an optional `asc` or `desc` afterwards to indicate direction. Here
are some examples:

* `lastName`
* `lastName desc`
* `lastName desc, firstName asc`

Note that not all fields can be sorted.

## Limit, Offset, and Total Count
When return a list of items, the `limit` parameter sets the maximum number of items that will be returned. The `offset` parameter specifies the index of the first item to be returned; this defaults to 0. Together, these parameters can be used for paging.

Calls to return a list of items also include an `X-Total-Count` header which returns the total number of items available (which could be used to determine the total number of pages, for instance). However, to improve performance, this won't return the full count, only enough to indicate if there are additional pages. A `maxCount` parameter can be provided which specifies what the highest number is that should be returned in `X-Total-Count`. By default, this value is determined automatially based on the `limit` and `offset`, and is returned in the `X-Total-Count-Max` header. In other words, if `X-Total-Count` equals `X-Total-Count-Max`, there are probably more results available than the number returned by `X-Total-Count`.

# Front-End / VueJS Notes

## Filtered Tables
Filtered tables are used for presenting lists of data. They allow for robust display, filtering, paging, searching, and exporting of tabular data. Filtered tables interact with the underlying API, and automatically build out URLs that account for whatever filtering, paging, etc. is chosen by the user.

The functionality of a given filtered table is defined in the ```settings``` object for the table. Here are a few of the key properties of that ```settings``` object:
* ```endpoint```: sets the API endpoint that the table interacts with. Usually, is an API prefix and entity name, such as ```/api/project``` or ```api/client``` for a table interacting with project or client data entities.
* ```includes```: an array of additional entities to include. By default, no related entities are returned by the API. If you need to include related objects, specify them here. You can use dotted syntax to follow multiple levels of a relationship. For instance: ```includes: ['people', 'people.address', 'people.employer']```
* ```getDefaultFilter```: a function that returns a default filter to apply. This lets you enforce a filter that the user can't control.
* ```showNewButton```: whether or not to show a "New" button on the toolbar for the table
* ```globalSearch```: whether or not to include a global search on the toolbar. This is disabled by default -- be careful, if can cause performance issues on large datasets.
* ```defaultSortColumn```: the default column to sort on.
* ```defaultSortOrder```: the default sort order, either ```asc``` or ```desc```.
* ```defaultLimit```: the default page size.

### Columns
The settings for a table define all the columns on the table. The required properties for each column are:
* ```key```: see below for more information.
* ```name```: the name the user sees for the column.
* ```type```: see below for more information.

Some additional properties that can be defined:
* ```visible```: whether or not the column is visible by default. If not, the user can choose to show it from the column menu above the table.
* ```sortable```: whether or not the column is sortable. If it is, the user can sort by clicking on the column header. For medium or large datasets, it's recommended to have an index defined for the underlying data entity for any sortable column.
* ```render(value, row)```: a function that allows you to provide custom rendering for the column. Both the individual cell value and the entire row are passed to the function. You can use this for formatting (e.g. formatting a date or phone number a certain way), it can also be used to create composite fields (e.g. a field that combines the values of a ```firstName``` and ```lastName``` field).
 
#### Key
The key identifies the name of the column in the underlying data that comes from an API. For instance:
* An entity/table called "Project" has a property called "Name".
* There's a corresponding DTO called "ProjectDTO" which also has a "Name" property.
* When serialized to JSON, the casing is changed to camel-casing, as is standard in JS. So on the client-side, the property is called "name".
* A filtered table has column with a "key" value of "name".

Filtered tables can follow relationships defined in the data model. For instance:
* An entity/table called "Project" has a property called "ClientId" and a corresponding "Client" property.
* The ApplicationDbContext defines the relationship between Project and Client.
* In the filtered table, there's a column with a "clientId" key.
* That column also has select options that use Vuex, reading the available options from a local store (or pulling from an API if they don't exist).

Filtered tables can follow properties on related entites. For instance:
* An entity/table called "Project" has a relationship to "Client". "Client" has a property called "Abbreviation".
* The filtered table has a column with a key of "client.abbreviation".
* The filtered table settings also include an ```include``` array, with ```client``` as a value in the array.

A key doesn't need to exist in the underlying data. If it doesn't, you should provide a custom ```render``` function.

#### Types
There are several types of columns. They have different ways of displaying the data, different filtering mechanisms, and different ways that the filters are ultimately serialized for the underlying API call.

* ```text```: used for basic text fields. By default, the applied filter performs a "startsWith" operation against the column. (This is translated to SQL as ```like 'filter%'```.)
  * ```filterMethod```: if set to ```contains```, the filter does a "contains" operation instead of "startsWith" (translated to SQL as ```like '%filter%'```).
* ```phone```: used for U.S. domestic phone number fields. Works similar to ```text```, but provides formatting for 10-digit numbers.
* ```select```: used for fields with a set of pre-defined options. The filter appears as a dropdown list. See the section below on select filters.
* ```multiselect```: used for fields with a set of pre-defined options, but with a one-to-many relationship. More than one value can be applied as part of the filter.
* ```guid```: used for GUID fields.
* ```number```: used for numeric types.
* ```date```: used for date fields. Uses a calendar widget for filtering. This can be used for datetime fields, but doesn't provide any time filtering.
* ```datetime```: used for datetime fields.
* ```datetimeoffset```: used for datetimeoffset fields.
* ```daterange```: used for date or datetime fields, when filtering needs to cover a span of days (rather than a single day, as with ```date```). The filter displays two calendar widgets.

### Select Filters
Select filters let you filter against a pre-defined list of options. Generally, there are two ways this is used:
* When the underlying data is an enum, so there's a constant set of possible values.
* When the underlying data is a relationship, so we need to make an API call to load the possible values.

The ```selectOptions``` property on the column can be used to provide a list of options. The provided options should be structured as an array of objects with properties of ```text``` and ```value```.

You can also use data managed by Vuex for columns by specifying a ```selectOptionsSource``` property.

For constant values, you'd specify a ```storeGetter``` on that object, like:
```
{
  key: 'state',
  name: 'State',
  visible: true,
  sortable: true,
  type: 'select',
  selectOptionsSource: { storeModule: 'cachedData', storeGetter: 'projectStateSelectOptions' }
}
```

For values that need to be loaded via API (related entities), specify the action (to load the entities) and the getter, like:
```
{
  key: 'clientId',
  name: 'Client',
  visible: true,
  sortable: true,
  type: 'select',
  selectOptionsSource: { storeModule: 'cachedData', storeAction: 'loadClients', storeGetter: 'clients' }
}
```

## Content
* There's basic content editing capabilities built in.
* Depending on the content (the "IsPage" field), content can be a unique page with its own URL or can just be embedded (using a content-block component) on a page.
* Content can also be used for non-web display, such as in emails or SMS messages.
* You can use Markdown formatting in content.

## Environment Settings
* You can use the .env file to create settings for the dev environment, and then override them in production by putting a different value in .env.production.
* This is explained in more detail at: https://cli.vuejs.org/guide/mode-and-env.html#modes

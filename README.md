# Welcome

This is a custom hobby project for myself, nothing special. It is fullly implemented by me, one developer only.

It is called **Talepreter**, an interpreter for tales which could be used for other purposes, such as movie making or other scenario required objects (books and novels mainly).

It is not entire code to make it more useful. It also has a WPF based GUI but not here due to some reasons. Maybe it will also be added later.

## What does it do?

Talepreter is a helper app to summarize what has happened in the tale, especially useful after maybe hundreds of pages. It stores information about actors, anecdotes (which could be anything), persons (non-actors, figurants), settlements and some world information. When I am writing *next* page, I need to know what has happened before, and if tale is very long then sometimes important information might be skipped. It would create inconsistencies if writer does not follow what has been mentioned before in former pages. 

> If actor X does not smoke (maybe never done in his life), in future pages tale should not mention that "X smokes a cigarette". Or similarly if actor Y does not have a driving license, in future pages tale should not mention him driving a car (for sure there might be exceptions like he is driving without a license, escaping from bad guys etc.); consistency would be broken for such cases. Maybe a settlement had a landslide some time ago and many things changed there, so in new pages the former information must be considered. If tale is long (like telling a tale of hundred years) then some people shall pass away eventually. It would be weird to mention actor X never ages and his mother is still living beyond limits of human life (unless the world has different rules). 

This app will **help** (not fix things automatically) writer on such topics given above. Writer can check for many details before writing the next page for the tale. At least it can look at Talepreter views to think and focus on real content instead of digging continuously to check if something is broken. For my own hobby, app does many other things too (many calculations) which I do not want to bother with personally. This does not mean it is limited to such features only but a starter for now.

# Tech Stack

Pure new stuff if possible, also some of them are new for me. I am a developer, not devops guy, so as long as stuff works, that is good enough.

* Uses netcore 8 mainly
* Uses Orleans, RabbitMQ, EntityFramework, Mongodb and maybe some other things I might add on the way (Kubernetes, more Mongodb). Maybe in very future time I might switch to Dapr or similar frameworks but later, when I have a need.
* Orchestration shall be basic docker-compose but maybe kubernetes yml will be added later. Everything is in container (except GUI)
* DB is SqlServer (MSSQL) in docker. It always has a temporary use for me, it can be deleted and regenerated anytime, due to secondary backup of the tales, that is another source control. Mongodb is used for final projection data.

GUI is different, it is WPF, and targets Windows OS. Talepreter runs in docker linux containers (5+ service for now) and GUI will be at Windows. 

Design has minor problems about decisions, some tech stack makes little sense or this app could be done with other frameworks easier. Answer is simple: I wanted to learn stuff. I already has future ideas with swapping some tech listed above, focusing on different frameworks but for now it will stay like this. First I want to see how its performance and flexibility is.

# Weird Stuff

This is a hobby project, most of the things are done on purpose to be faster developed. If it is working good *enough*, then I am happy *enough*. If it works on my machine that means it is done.

Some parts of code may not be visible. I believe not but they are required in most of these.

GUI is in WPF so targets windows for now, maybe in the future it can be a web app, but due to my alternate content backup (filesystem), it will be a little weird.

My content (content of a tale, scenario for example) is always in a file system supported by a source control, so losing entire data in Talepreter is very normal (part of an action I do perhaps daily, not about development). The entire data will be read from file system and regenerated again (and again and again), which might be a little weird compared to other apps in other businesses.

> When I write a tale, I continuously go back and add stuff to former pages. From an author perspective, it is very weird. A completed page must not be touched, but I do because I am not a good writer. This makes me forced to build views again and again, sometimes disconnect parts of the tale to rewrite some time later. It means a (even first) page is never complete, even after I am writing thousandth page. Next day I might go back to a former page, change it, and have to see new results. This means entire view data (of services except talesvc) must be regenerated again. That is the main use case for the app.

The app existed before but a single desktop app, monolith doing everything. Now it is time to move to proper microsvc architecture as much as possible. 

Some data modeling is weird but it is done on purpose. The content of a tale is in text and the notes coming from a content must be humanly readable/writable (even json is very unreadable for that), so there are many generic rules to support all kinds of notes (I call them page command). It is not a good idea for a system like that perhaps but again the main purpose is my own usage and I write tales in markdown, so design of app is very affected by that too, simple text processing mainly.

## Simplification & Shortcuts

There are some places where the application does weird things but also I could not find a better way. The source of commands in a tale is single and same for every service. So same command will go to four services to be processed. Problem starts there, not every service is interested in those commands, and furthermore the plugin system makes it very open-ended. While I implement base, I know which commands will go to which service, but then plugins I think of comes and ruins that hard-coded expectations. As of writing the application I think of separating my single monolithic plugin into multiple smaller parts too so this will make the command-service relation very decoupled, almost no relation there. To solve that I invented my way of global broadcast like responses. It also solves my feature intention of GUI progress bar. I want to see numbers in GUI and measure stuff there. due to this problem, the response system is generated. 

By simplification I could simply go forward with ditching plugin system completely. Then system would know which commands are executed in which service. "processing" would be minimal too, instead we know exactly what will be executed. 

This simplification could be done in other ways too but I chose this way

## Performance Concerns

There are some parts which I have concerns about, performance of grains and being blocked by common DB. In my previous application everything was in memory but also process was done in single thread. Since it was single application, I did not need to have a concept of communication. Now with multiple services, there is a concept and furthermore these worker entities are orleans actors, which means they will be blocked down to single thread sometimes. I have taken some shortcuts about validation to make things faster already. Full publish operation in old application takes up to 3 seconds in my sample tale of 30 chapters/660 pages/7k page commands. That means (more than) 7k message will be processed during a full publish operation. Same grains will be called many times over things to update (about progress) and response handling. Due to this huge message handling I focused on progress response handling, like how much is done within a second, maybe how long a full publish takes. 

There could be other ways to handle this too, but I chose that way to see if it is that bad or negligible. One of the open issue is writing (upload operation) page commands, they go directly into services. Chapter/page grains are only used to process/execute/publish responses, not write. The very obvious solution would be not to upload everything again but the problem comes from changes (in past pages) I do naturally. I change past of the pages so most operations (process/execute/publish) has to be done again. That direction (changing what is needed only) will be next version I will implement, and maybe go with full actor model depending on performance. Without plugin concept, this process would be simplified too.

My example tale has 30 chapters and 660 pages right now, 7k commands in them. I checked the services about how many commands were executed in total. 7k commands are scattered to four services in total about 13k commands (doubled in number). If I execute entire tale to create data from scratch it takes 15 minutes (which is very high) but considering it really executed 13k separate commands (of all of them are DB operations) I think it is fine. I also did not optimize fully yet (I think of compound operations, which take multiple commands per grain call so the overhead will be shorter there). Single page with 10-20 commands takes a second to execute in all services so it is good enough for me. My machine is a potato too (7 years old) so I take it as a good enough result to stop here.

# Questions About Code or Design

Q: Dude, use brackets!
A: Yea, I know. Best practice is to use them but I have my personal feeling and reasoning to skip them. I am really very comfortable on reading code actually. That is not how I write code in work.

Q: There is a better way for this/that...
A: Sure, but as of writing I did not find/know a better way and also my requirements are absolute for me instead of following industry practices. I am aware of some sections which can be improved. Main reason was to be able to share something without revealing everything. I also have time constraints.

Q: There are many unused stuff.
A: True, because I changed design three times during development due to issues and things I learned. There are some parts which are nasty in tech stack, like Orleans, EntityFramework and RabbitMQ, and due to these I had to step back and change design. Performance is still not good enough for me but as is it is working and fine enough for first version. First design used RabbitMQ heavily but nothing about Orleans. I wanted Orleans so I integrated it. Performance was terrible due to bottlenecks with Orleans. This is the third design which compromises many things sadly.

Q: Any plans for GUI or next version?
A: GUI is very specific to my plugins and I am not interested in sharing them (they are the real deal actually, covering many open spots the base infrastructure does not do). Next version will happen since I know more now but then it will not be shared because I think of ditching the plugin idea fully and handling everything as part of design. [Here](gui-sample.png) is a sample for my GUI. All top words are tabs and at each tab user can see data accumulated (loaded from Mongodb). Progression is short summary and Preview is for next page (helpers about validation and such). The controls are custom made (I implemented them) and might be confusing to someone who does not know them sadly.

Q: No tests?
A: Yes unfortunately. I tested this fully though. Remember my words above, I have another application that does the **exact** same thing but a monolith. So I compare results in both views (GUI is %99 same, the data is read from Mongodb, which is the different part). Their data model is same code so testing was very simple, just compare two models coming from two applications. It is not an excuse though, I will add more tests for next version.

Q: This is in one big commit.
A: True, the repo "talepreter-public" implies there is a "talepreter-private". There goes entire development including plugins and GUI. I just ripped them off to post here in this repo.

Q: No different repos? Just one?
A: I would love to have a proper project structure where every service is in its own repo but due to my incompetence or my github account's limitations, I cannot do that. I could not find a way, otherwise I would do it. It has merits and perils.

Q: How can I execute this thing?
A: Sadly it is not so simple because it lacks a GUI or any way to interact with the application. It is actually kind of simple to make it run though. Build everything in docker, code should compile. Create an ".env" file with all parameters needed in docker-compose file. Then create DB migration containers. Run all migrations, they create DBs and such. Then create services and that is all. There is no communication channel, things go with Orleans grains (my shortcut, it should not be that way). I shared the project only, not trying to add more developers to this. 

Q: Any flow or sample?
A: If you managed to run everything then the thing you need to do is to use *ITaleGrain*. That is the entry point and you do not need to interact with rest. Call Initialize on it to set a new publish grain. Flow is always same: AddChapterPage >> BeginProcess (listen results from RabbitMQ, if all success move to next step) >> BeginExecute (same...) >> AddChapterPage... and so on. If done with all process and execute then BeginPublish. Once it gives response (with RabbitMQ messages) then your data is ready in Mongodb. 

Q: Some design explanation? Publish? Write? Process?
A: **Tale** is root, and singleton, representing your tale project. System supports multiple tales just fine but will not mix them. **Publish** (bad naming) is representing each different version of it, while it might use same data (pages). It is like second publish, third publish of a tale. I switched to *Version* later but it is not used everywhere. **Command** is a note in every page, the core atomic request to service actors to do something. *PageCommand* is a wording I used for that too but it changed name where it is used because it included more data or less, also it goes to old design. I named the operations **Write=Process**, **Execute**, and **Publish** (sadly same name here). *Write* comes from old version, it should be **Process**. Every command is processed at each service. It is like pre-inspection, to see if services (or plugins) will add more commands due to the commands coming. It is possible and a good use case, plugins are interested in these commands and they create their own commands during processing. It is like a bucket at the end, a bucket of commands at each service. Also main validations go here. Process does nothing to entities, it is **Execute** that does that. Execute applies the changes requested by each command to the entities, which can be *Actor*, *Anecdote*, *Person* and so on. At the end of Execute each affected entity will have a changed version (a projection like) but for simplicity system keeps one version of it per publish. Each page goes through these steps. Once everything is done, **Publish** can occur, which means "*take the entities' final versions and put them elsewhere*" (which is Mongodb). Published entities are readonly, they are final. Publishes at any time can be deleted, re-created and so on. Entities like *Actor*, *Anecdote* are handling idempotency and concurrency issues mainly, other than that it is the bottleneck of application. Each processed command targets a single grain so every command can run in parallel only if their targets are unique and different. If they are same then they have to wait each other (which is the bottleneck of the application but it solves concurrency issues very well). System has limitations per design. Only one writer can exist and work on a tale, also system assumes there is only one client (shortcuts of my design, but I have no other use-case either).

Q: I have this/that idea to improve this.
A: Code is open so create a PR, I might look into it. If it is way too bigger then contact me at gungorenu@gmail.com for anything else.


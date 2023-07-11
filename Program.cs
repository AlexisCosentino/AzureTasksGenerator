// See https://aka.ms/new-console-template for more information
using CreateTaskFromIteration;
using System.Net.Sockets;
using Spectre.Console;
using System.Security.Cryptography.X509Certificates;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

AnsiConsole.Write(
    new FigletText("AZURE TASKS")
        .LeftJustified()
        .Color(Color.Green4));
Thread.Sleep(1000);
Console.WriteLine("");
Console.WriteLine("");

AnsiConsole.MarkupLine("[underline reverse green]Hi there[/]");
Console.WriteLine("");
Thread.Sleep(1000);
AnsiConsole.MarkupLine("[green]You will duplicate your PBI into Tasks[/]");
Console.WriteLine("");
bool ready = AnsiConsole.Confirm("[green]R U ready ?[/]");

if (ready)
{
    AnsiConsole.Status()
   .Start("Connection", ctx =>
   {
       AnsiConsole.MarkupLine("Connecting to azure organisation...");
       Thread.Sleep(3000);
       // Update the status and spinner
       ctx.Status("Next task");
       ctx.Spinner(Spinner.Known.Aesthetic);
       ctx.SpinnerStyle(Style.Parse("green"));
   });
   Console.WriteLine("");
   bool date = AnsiConsole.Confirm("[green]Today is your current sprint ?[/]");
    if (date)
    {
        var project = askProject();
        confirmProject(project);

    } else
    {
        AnsiConsole.MarkupLine("[green]SORRY, YOU [reverse] MUST [/] DO IT DURING YOUR CURRENT SPRINT[/]");
        AnsiConsole.MarkupLine("[green]Come back later ![/]");
        Thread.Sleep(3000);
        return;
    }
}
else
{
    AnsiConsole.MarkupLine("[green reverse]CIAO ![/]");
    Thread.Sleep(1000);
    return;
}

string askProject()
{
    return AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("[green] Select your Azure project :[/]")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more projects)[/]")
        .AddChoices(new[] {
            "TEST_ALEXIS", "Mobility", "Digital", "Locpro"
        }));
}

void confirmProject(string project)
{
    bool confirm = AnsiConsole.Confirm($"[green]You chose [reverse green] {project} [/], do you confirm ?[/]");
    if (confirm)
    {
        letsGo(project);
    }
    else
    {
        project = askProject();
        confirmProject(project) ;
    }
}

void letsGo(string project)
{
    var get = new GetAzure();
    get.url = $"https://dev.azure.com/IRIUMSOFTWARE/{project}/_apis/work/teamsettings/iterations?$timeframe=current&api-version=7.0";
    var result = get.GettingFromAzure();
    get.url = $"https://dev.azure.com/IRIUMSOFTWARE/{project}/_apis/work/teamsettings/iterations/{result.value[0].id}/workitems?api-version=7.0";
    result = get.GettingFromAzure();
    AnsiConsole.MarkupLine("[green]Here are the IDs of your current sprint's PBI : [/]");
    foreach (var item in result.workItemRelations)
        AnsiConsole.Markup($"[green]{item.target.id}, [/]");

    bool confirm = AnsiConsole.Confirm("[green]Do you confirm ?[/]");
    if (confirm)
    {
        foreach (var item in result.workItemRelations)
        {
            get.url = item.target.url + "?$expand=All&api-version=6.1-preview.3";
            result = get.GettingFromAzure();
            bool hasChildRelation = false;
            bool isItATask = false;

            //Check if pbi has already a task. Need check if there is a child relation
            foreach (var relation in result.relations ?? new dynamic[0])
            {
                if (relation.rel == "System.LinkTypes.Hierarchy-Forward")
                {
                    hasChildRelation = true;
                    break;
                }
            }

            Console.WriteLine($"------------------{result.fields["System.WorkItemType"]}--------------");

            //Check if it is not already a task but real PBI that need duplication
            if (result.fields["System.WorkItemType"] == "Task")
            {
                isItATask = true;
            }



            if (hasChildRelation == false && isItATask == false)
            {
                var json = new CreateJsonBody();
                json.ticket = result;

                var post = new PostToAzure();
                post.url = $"https://dev.azure.com/IRIUMSOFTWARE/{project}/_apis/wit/workitems/$Task?api-version=7.0";
                post.json = json.createJsonWithPBIToPostFromDynamic();
                var newResult = post.postingToAzure();

                //Get & Patch relation hierarchy
                var patch = new PatchToAzure();
                patch.url = $"https://dev.azure.com/IRIUMSOFTWARE/{project}/_apis/wit/workitems/{newResult.id}?api-version=7.0";
                patch.json = "[{\"op\": \"add\", \"path\": \"/relations/-\", \"value\": {\"rel\": \"System.LinkTypes.Hierarchy-Reverse\",  \"url\": \" " + item.target.url + " \"}}]";
                patch.patchingToAzure();
            }
        }
    } else
    {
        return;
    }
}

// Checker si il y a un lien enfant parent et si oyui, ne pas créer de tache.
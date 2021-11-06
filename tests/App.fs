module App

open Elmish
open Feliz
open Feliz.Bulma
open Fable.Core

[<RequireQualifiedAccess>]
type Page =
    | Home
    | About

type Model =
    {
        CurrentRoute : Router.Route option
        ActivePage : Page
        Value : int
    }

type Msg =
    | Tick of unit

let private setRoute (result: Option<Router.Route>) (model : Model) =
    let model = { model with CurrentRoute = result }

    match result with
    | None ->
        let requestedUrl = Browser.Dom.window.location.href

        JS.console.error("Error parsing url: " + requestedUrl)

        { model with
            ActivePage = Page.Home
        }
        , Cmd.none

    | Some route ->
        match route with
        | Router.Home ->
            { model with
                ActivePage = Page.Home
            }
            , Cmd.none

        | Router.About ->
            { model with
                ActivePage = Page.About
            }
            , Cmd.none


let private init (optRoute : Router.Route option) =
    {
        CurrentRoute = None
        ActivePage = Page.Home
        Value = 0
    }
    |> setRoute optRoute

let private tick () =
    promise {
        do! Promise.sleep 1000

        return ()
    }

let private update (msg : Msg) (model : Model) =
    match msg with
    | Tick _ ->
        { model with
            Value = model.Value + 1
        }
        , Cmd.OfPromise.perform tick () Tick

let private liveCounter model =
    Bulma.hero [
        hero.isLarge

        prop.children [
            Bulma.heroBody [
                Bulma.text.div [
                    text.hasTextCentered
                    prop.style [
                        style.width (length.percent 100)
                    ]

                    prop.children [
                        Html.div [
                            Html.text "Application is running since "
                            Html.text (string model.Value)
                            Html.text " seconds"
                        ]

                        Html.br [ ]

                        Html.text "Change me and check that the timer has not been reset"
                    ]
                ]
            ]
        ]
    ]

let private navbar (model : Model) =
    Bulma.navbar [
        color.isInfo

        prop.children [
            Bulma.navbarMenu [
                prop.children [
                    Bulma.navbarStart.div [
                        prop.children [
                            Bulma.navbarItem.a [
                                Router.href Router.Home

                                prop.text "Home"
                            ]

                            Bulma.navbarItem.a [
                                Router.href Router.About

                                prop.text "About"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let private renderActivePage (page : Page) =
    let content =
        match page with
        | Page.Home ->
            Bulma.text.p [
                text.hasTextCentered

                prop.children [
                    Html.text "This is the home page"
                ]
            ]

        | Page.About ->
            Bulma.text.p [
                text.hasTextCentered

                prop.children [
                    Html.text "This is the about page"
                ]
            ]

    Bulma.section [
        content
    ]

let private view (model : Model) (dispatch : Dispatch<Msg>) =
    Html.div [
        navbar model

        renderActivePage model.ActivePage

        Bulma.container [
            liveCounter model
        ]
    ]

open Elmish.UrlParser
open Elmish.HMR

// Use a subscription to trigger the Tick system
// If we try to trigger the ticks from the Update function
// This doesn't work because dispatch instance available in the waiting tick refers
// to the old programs meaning that the new instance never "Tick"

// Also, we are not using Hooks for the Tick system because we want to test Elmish HMR
// supports and not React HMR or React fast refresh

// Trigger a Tick on program launch to start the timer
let tickSubscription _ =
    Cmd.ofMsg (Tick ())

Program.mkProgram init update view
|> Program.withSubscription tickSubscription
|> Program.toNavigable (parseHash Router.pageParser) setRoute
|> Program.withReactBatched "root"
|> Program.run

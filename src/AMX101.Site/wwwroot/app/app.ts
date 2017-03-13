declare var window: any;
// Controllers
import { SearchController } from "./controllers/searchController";
import { ResultController } from "./controllers/resultController";
import { CompareController } from "./controllers/compareController";

// Services
import { SearchService } from "./services/searchService";
import { ClaimService } from "./services/claimService";
import { ComparisonService } from "./services/comparisonService";

// Directives
import { FooterDirective } from "./directives/footer.directive";
import { ImageModalDirective } from "./directives/imageModal.directive";

angular.module("postcodeApp", ["ui.router"])
    .controller("searchController", SearchController)
    .controller("resultController", ResultController)
    .controller("compareController", CompareController)

    .service("searchService", SearchService)
    .service("claimService", ClaimService)
    .service("comparisonService", ComparisonService)

    .directive("termsService", FooterDirective.factory)
    .directive("imageModal", ImageModalDirective.factory)

    .constant("myConfig",
    {
        region: window.__env.region,
        onlineMode: window.__env.onlineMode
    })

    .config(($stateProvider, $urlRouterProvider, myConfig) => {
        let rootDir = "/app/views";

        $urlRouterProvider.otherwise("/");

        $stateProvider
            .state("search",
            {
                url: "/",
                templateUrl: rootDir + "/search.html",
                controller: "searchController",
                params: {
                    region: myConfig.region,
                    onlineMode: myConfig.onlineMode,
                    postcode: null
                }
            })
            .state("result",
            {
                url: "/result/:region/:postcode",
                params: {
                    region: myConfig.region,
                    onlineMode: myConfig.onlineMode,
                    postcode: null
                },
                templateUrl: rootDir + "/result.html",
                controller: "resultController"
            })
            .state("compare",
            {
                url: "/compare/:region/:postcode",
                params: {
                    region: myConfig.region,
                    onlineMode: myConfig.onlineMode,
                    postcode: null
                },
                templateUrl: rootDir + "/compare.html",
                controller: "compareController"
            });
    });

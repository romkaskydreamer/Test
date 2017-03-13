import { IClaimService } from "../services/interfaces";
import { Source } from "../models/source";

interface IFooterScope extends ng.IScope {
    isExpanded: boolean;
    sources: Source[];
    region: string;
    toggle(): void;
}

class FooterDirective implements ng.IDirective {
    public static transclude = false;

    public static factory = [
        "claimService",
        "$state",
        (claimService: IClaimService, $state: ng.ui.IStateService) => { return new FooterDirective(claimService, $state); }];

    public templateUrl = "/app/directives/footer.directive.html";

    public constructor(private claimService: IClaimService, private $state: ng.ui.IStateService) {

    }
    public link = ($scope: IFooterScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
        $scope.$on("showTerms",
            () => {
                $scope.isExpanded = true;
            });

        $scope.region = this.$state.params["region"];

        $scope.isExpanded = false;
        $scope.toggle = () => {
            $scope.isExpanded = !$scope.isExpanded;
        };

        this.claimService.getSources($scope.region)
            .then(result => {
                $scope.sources = result;
            });
    }

}

export { FooterDirective };

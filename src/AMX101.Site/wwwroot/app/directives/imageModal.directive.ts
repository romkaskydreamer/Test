import { IClaimService } from "../services/interfaces";
import { ImageClaim } from "../models/Claim";
import { PopulatedClaim } from "../models/populatedClaim";

interface IImageModalScope extends ng.IScope {
    visible: boolean;
    claims: ImageClaim[];
    currentClaims: number[];
    currentCommonClaims: PopulatedClaim[];
    postcode: string;
    btnDisabled: boolean;
    region: string;
    closeModal(): void;
    confirmDownload(): void;
    toggleClaim(claim: ImageClaim): void;
}

class ImageModalDirective implements ng.IDirective {
    public static transclude = false;

    public static factory = [
        "claimService",
        "$timeout",
        "$state", (
            claimService: IClaimService,
            $timeout: ng.ITimeoutService,
            $state: ng.ui.IStateService
        ) => new ImageModalDirective(claimService, $timeout, $state)
    ];

    public templateUrl = "/app/directives/imageModal.directive.html";

    public scope = {
        visible: "=",
        currentClaims: "=",
        currentCommonClaims: "=",
        postcode: "="
    };

    private $scope: IImageModalScope;

    public constructor(
        private claimService: IClaimService,
        private $timeout: ng.ITimeoutService,
        private $state: ng.ui.IStateService) {

    }

    public link = (scope: IImageModalScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
        this.$scope = scope;
        this.$scope.region = this.$state.params["region"];

        this.claimService.getImageClaims(this.$scope.region)
            .then(result => {
                this.$scope.claims = result;
                this.$scope.claims.forEach(item => {
                    let current = this.$scope.currentCommonClaims.some(claim => {
                        return claim.id === item.id;
                    });
                    if (current) {
                        item.selected = true;
                    } else {
                        item.selected = false;
                    }

                });
            });

        this.$scope.closeModal = () => {
            this.$scope.visible = false;
            this.$scope.btnDisabled = false;
        };

        this.$scope.confirmDownload = () => {
            if (!this.$scope.btnDisabled) {
                this.$scope.btnDisabled = true;
                let url = "/api/claims/image?postcode=" + this.$scope.postcode;
                let selectedClaims = this.$scope.claims.filter(item => {
                    return item.selected;
                });
                selectedClaims.forEach(item => {
                    url += "&staticClaimIds=" + item.id;
                });

                this.$scope.currentClaims.forEach(item => {
                    url += "&claimIds=" + item;
                });

                this.$timeout(() => {
                    this.$scope.closeModal();
                }, 3000);

                window.location.href = url;
            }
        };

        this.$scope.toggleClaim = (claim: ImageClaim) => {
            let index = this.$scope.claims.indexOf(claim);
            this.$scope.claims[index].selected = !this.$scope.claims[index].selected;
        };
    }
}

export { ImageModalDirective };

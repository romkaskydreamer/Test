import { ISearchService } from "../services/interfaces";
import { IClaimService } from "../services/interfaces";
import { StaticClaim } from "../models/staticClaim";
import { PopulatedClaim } from "../models/populatedClaim";
import { Industry } from "../models/industry";
import { Type, Regions } from "../models/enums";

interface IResultScope extends ng.IScope {
    isLoading: boolean;
    isModalVisible: boolean;
    region: string;
    onlineMode: boolean;
    map: google.maps.Map;
    lat: number;
    lng: number;
    postcode: string;
    tempPostcode: string;
    staticClaim: PopulatedClaim;
    dynamicPrimaryClaims: PopulatedClaim[];
    selectedPrimaryClaim: PopulatedClaim;
    dynamicSecondaryClaims: PopulatedClaim[];
    selectedSecondaryClaim: PopulatedClaim;
    commonClaims: StaticClaim[];
    commonClaimPool: StaticClaim[];
    selectedCommonClaims: StaticClaim[];
    industries: Industry[];
    selectedIndustry: Industry;
    postcodeInvalid: boolean;
    noDataMessage: boolean;
    selectedClaimsForModal: number[];

    displayModal(): void;
    openSources(): void;
    validate(query: string, callback: any);
    updateModalClaims(): void;
    changeIndustry(industry: Industry): void;
    refreshClaim(index: number): void;
    renderPdf(): void;
    renderImage(): void;
    getStaticMapUrlFromPostCode(postCode: number, region: string): string;
    updateMap(): void;
    getLatLong(): void;
    loadTiles(): void;
    init(): void;
    initMap(): void;
}

class ResultController {
    private _minCards: number;

    constructor(
        private $scope: IResultScope,
        private $state: ng.ui.IStateService,
        private claimService: IClaimService,
        private searchService: ISearchService) {

        // Set the modal as invisible for the export to jpg functionality
        this.$scope.isModalVisible = false;
        this.$scope.isLoading = false;
        this.$scope.postcodeInvalid = false;
        this.$scope.noDataMessage = false;
        this.$scope.selectedClaimsForModal = <number[]>[];
        // init
        this.$scope.region = this.$state.params["region"];
        this.$scope.postcode = this.$state.params["postcode"];
        this.$scope.onlineMode = this.$state.params["onlineMode"];

        this.$scope.tempPostcode = this.$scope.postcode;
        this.$scope.industries = <Industry[]>[
            {
                name: "All Industries",
                value: 0
            },
            {
                name: "Retail",
                value: 1
            },
            {
                name: "Dining",
                value: 2
            },
            {
                name: "Lodge",
                value: 3
            }];

        this.$scope.dynamicPrimaryClaims = <PopulatedClaim[]>[];
        this.$scope.dynamicSecondaryClaims = <PopulatedClaim[]>[];
        this.$scope.commonClaims = <StaticClaim[]>[];
        this.$scope.selectedCommonClaims = <StaticClaim[]>[];
        this.$scope.$watch("tempPostcode",
            () => {
                if (this.$scope.tempPostcode.length >= 2) {
                    this.$scope.postcodeInvalid = false;
                    this.$scope.noDataMessage = false;
                    this.$scope.validate(this.$scope.tempPostcode,
                        (isValid) => {
                            if (isValid) {
                                searchService.isValidPostcode(this.$scope.tempPostcode, this.$scope.region)
                                    .then(valid => {
                                        if (valid) {
                                            this.$scope.postcode = this.$scope.tempPostcode;
                                            this.$scope.loadTiles();
                                        } else {
                                            this.$scope.isLoading = false;
                                            this.$scope.noDataMessage = true;
                                        }
                                    });

                            } else {
                                this.$scope.isLoading = false;
                                this.$scope.postcodeInvalid = true;
                            }
                        });
                }
            });

        this.$scope.changeIndustry = (industry: Industry) => {
            // change the industry
            this.$scope.selectedIndustry = industry;
            // change the first dynamic tile
            this.$scope.selectedPrimaryClaim = this.$scope.dynamicPrimaryClaims.filter(item => {
                return item.industry === industry.value;
            })[0];
            // change the second dynamic tile
            this.$scope.selectedSecondaryClaim = this.$scope.dynamicSecondaryClaims.filter(item => {
                return item.industry === industry.value;
            })[0];
            this.$scope.updateModalClaims();
            // reorder the pool of static claims to prioritise the selected industry
            this.$scope.commonClaimPool.sort((a, b) => {
                let aCat = a.category === industry.value;
                let bCat = b.category === industry.value;

                if ((aCat && bCat) || (!aCat && !bCat)) {
                    return 0;
                }

                if (aCat && !bCat) {
                    return -1;
                }

                return 1;
            });
        };

        this.$scope.init = () => {
            claimService.getStaticClaims(this.$scope.region)
                .then(result => {
                    this.$scope.commonClaims = result;
                    this.$scope.selectedCommonClaims = this.$scope.commonClaims.slice(0, 3);
                    this.$scope.commonClaimPool = this.$scope.commonClaims.slice(3);
                });
        };

        // run the update function
        this.$scope.loadTiles = () => {
            this.$scope.isLoading = true;
            this.$scope.dynamicPrimaryClaims.length = 0;
            this.$scope.dynamicSecondaryClaims.length = 0;
            this.$scope.commonClaims.length = 0;

            this.$scope.selectedIndustry = this.$scope.industries[0];

            claimService.getPopulatedClaimsByPostcode(this.$scope.postcode, this.$scope.region)
                .then(result => {
                    result.forEach(claim => {
                        switch (claim.type) {
                            case Type.Static:
                                this.$scope.staticClaim = claim;
                                break;
                            case Type.DynamicPrimary:
                                this.$scope.dynamicPrimaryClaims.push(claim);
                                break;
                            case Type.DynamicSecondary:
                                this.$scope.dynamicSecondaryClaims.push(claim);
                                break;
                            default:
                                console.log("No type found:");
                                console.dir(claim);
                                break;
                        }
                    });

                    this.$scope.selectedPrimaryClaim = this.$scope.dynamicPrimaryClaims.filter(item => {
                        return item.industry === $scope.selectedIndustry.value;
                    })[0];
                    this.$scope.selectedSecondaryClaim = this.$scope.dynamicSecondaryClaims.filter(item => {
                        return item.industry === $scope.selectedIndustry.value;
                    })[0];

                    this.$scope.updateModalClaims();
                    this.$scope.isLoading = false;
                });
        };

        this.$scope.refreshClaim = (index: number) => {
            this.$scope.selectedCommonClaims[index].isRefreshing = true;
            let newClaim = this.$scope.commonClaimPool.splice(0, 1);

            let oldClaim = this.$scope.selectedCommonClaims.splice(index, 1, newClaim[0]);

            oldClaim.forEach(item => {
                this.$scope.commonClaimPool.push(item);
            });
            this.$scope.selectedCommonClaims[index].isRefreshing = false;
        };

        this.$scope.init();

        this.$scope.renderPdf = () => {
            let staticClaimIds = this.$scope.selectedCommonClaims.map(item => {
                return item.id;
            });

            let claimIds = [];

            claimIds.push(this.$scope.staticClaim.id);

            if (this.$scope.selectedPrimaryClaim) {
                claimIds.push(this.$scope.selectedPrimaryClaim.id);
            }

            if (this.$scope.selectedSecondaryClaim) {
                claimIds.push(this.$scope.selectedSecondaryClaim.id);
            }

            let url = `/api/claims/pdf?postcode=${this.$scope.postcode}&industry=${$scope.selectedIndustry.value}&region=${this.$scope.region}`;

            staticClaimIds.forEach(item => {
                url += "&staticClaimIds=" + item;
            });
            claimIds.forEach(item => {
                url += "&claimIds=" + item;
            });
            window.location.href = url;
        };

        this.$scope.renderImage = () => {
            let staticClaimIds = this.$scope.selectedCommonClaims.map(item => {
                return item.id;
            });
            let claimIds = [];
            claimIds.push(this.$scope.staticClaim.id);
            if (this.$scope.selectedPrimaryClaim) {
                claimIds.push(this.$scope.selectedPrimaryClaim.id);
            }
            if (this.$scope.selectedSecondaryClaim) {
                claimIds.push(this.$scope.selectedSecondaryClaim.id);
            }

            let url = `/api/claims/image?postcode=${this.$scope.postcode}&lat=${this.$scope.lat}&lng=${this.$scope.lng}&region=${this.$scope.region}`;
            staticClaimIds.forEach(item => {
                url += "&staticClaimIds=" + item;
            });
            claimIds.forEach(item => {
                url += "&claimIds=" + item;
            });
            window.location.href = url;
        };

        this.$scope.getStaticMapUrlFromPostCode = this.getStaticMapUrlFromPostCode;

        this.$scope.updateModalClaims = () => {
            this.$scope.selectedClaimsForModal = <number[]>[];
            this.$scope.selectedClaimsForModal.push(this.$scope.staticClaim.id);
            this.$scope.selectedClaimsForModal.push(this.$scope.selectedPrimaryClaim.id);
            this.$scope.selectedClaimsForModal.push(this.$scope.selectedSecondaryClaim.id);
            console.dir(this.$scope.selectedClaimsForModal);
        };

        this.$scope.openSources = () => {
            this.$scope.$emit("showTerms");
        };
        this.$scope.displayModal = this.displayModal;

        this.$scope.validate = (query: string, callback: any) => {
            if (this.$scope.region !== Regions.Singapore.toString()) {
                if (this.$scope.onlineMode) {
                    this.googleValidation(callback);
                } else {
                    this.offlineValidation(query, searchService, callback);
                }
            }
            else {
                this.customValidation(query, callback);
            }
        };
    }

    private offlineValidation = (query: string, searchService: ISearchService, callback: any) => {
        searchService.isValidPostcode(query, this.$scope.region)
            .then((valid) => {
                callback(valid);
            });
    }

    private customValidation = (query: string, callback: any) => {
        try {
            let postcodePrefix = parseInt(query);
            callback((postcodePrefix > 0) && (postcodePrefix < 84));
        }
        catch (error) {
            callback(false);
        }
    }

    private googleValidation = (callback: any) => {
        let autocomplete = new google.maps.places.AutocompleteService();
        let places = new google.maps.places.PlacesService(new google.maps.Map(document.getElementById("map")));

        let request = <google.maps.places.AutocompletionRequest>{
            input: this.$scope.postcode,
            componentRestrictions: <google.maps.places.ComponentRestrictions>{
                country: this.$scope.region.substr(0, 2).toUpperCase()
            },
            types: ["(regions)"]
        };

        autocomplete.getPlacePredictions(request,
            (results) => {
                if (results && results.length > 0) {
                    let resultNumber = results.length;
                    let locations = [];
                    results.forEach((res, index) => {
                        places.getDetails({ placeId: res.place_id },
                            (place, b) => {
                                let postcode = place.address_components.filter(item => {
                                    return item.types.indexOf("postal_code") > -1;
                                })[0];
                                if (postcode) {
                                    locations.push(postcode.short_name);
                                }
                                if (index === resultNumber - 1) {
                                    let valid = locations.indexOf(this.$scope.postcode) > -1;
                                    callback(valid);
                                }
                            });
                    });
                } else {
                    callback(false);
                }
            });
    }

    private getStaticMapUrlFromPostCode(postCode: number, region: string) {
        // Returns the map image according to the postal code supplied
        let center = "";
        switch (region) {
            case Regions.Australia.toString():
                center = "Australia,";
                break;
            case Regions.NewZealand.toString():
                center = "New Zealand,";
                break;
            case Regions.Singapore.toString():
                center = "Singapore,";
                break;

            default:
                break;
        }

        return "https://maps.googleapis.com/maps/api/staticmap?key=AIzaSyCKDgyXKZyjoieQyRUFGDPXPVHtcCEKYgI&center=" + center + postCode + "&zoom=14&size=500x219&scale=2&maptype=roadmap&style=feature:all|element:geometry|color:0x174972&style=feature:all|element:geometry.stroke|weight:1.07|lightness:7&style=feature:all|element:labels.text|visibility:off&style=feature:administrative|visibility:off&style=element:labels|visibility:off&style=feature:landscape|element:all|color:0x1b68b2&style=feature:landscape.man_made|element:all|visibility:off&style=feature:poi|element:geometry|lightness:-7&style=feature:road|element:all|visibility:simplified&style=feature:road|element:labels|visibility:off&style=feature:transit|element:all|visibility:off&style=feature:water|element:all|lightness:-24";
    }

    private displayModal = () => {
        // OPen the modal
        this.$scope.isModalVisible = true;
    }
}

export { ResultController };

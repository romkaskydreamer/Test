import { ISearchService } from "../services/interfaces";
import { AutocompleteResult } from "../models/autocompleteResult";
import { Config } from "../models/config";
import { Type, Regions } from "../models/enums";

interface ISearchScope extends ng.IScope {
    searchQuery: string;
    autocompleteResults: AutocompleteResult[];
    errorMessage: boolean;
    noDataMessage: boolean;
    isLoading: boolean;
    isSearching: boolean;
    validated: boolean;
    configOverride: string;
    region: string;
    onlineMode: boolean;
    selectPostcode(result: AutocompleteResult): void;
    validate(query: string, callback: any): void;
    validateAndSubmit(): void;
    changeText(): void;
}

class SearchController {

    constructor(
        private $scope: ISearchScope,
        private searchService: ISearchService,
        private $state: ng.ui.IStateService,
        private myConfig: Config) {

        this.$scope.searchQuery = "";
        this.$scope.configOverride = myConfig.region;

        if (this.$scope.configOverride !== "") {
            this.$scope.region = this.$scope.configOverride;
        }
        else {
            this.$scope.region = Regions.Australia.toString();
        }

        this.$scope.isSearching = false;
        this.$scope.onlineMode = this.$state.params["onlineMode"];

        this.$scope.$watch("searchQuery",
            () => {
                if (this.$scope.searchQuery && this.$scope.searchQuery.length > 0) {
                    this.$scope.isSearching = true;
                    this.$scope.errorMessage = false;
                    this.$scope.noDataMessage = false;
                    this.$scope.autocompleteResults = [];
                    if (this.$scope.region !== Regions.Singapore.toString()) {
                        if (this.$scope.onlineMode) {
                            this.googleAutocomplete();
                        } else {
                            this.offlineAutocomplete(searchService);
                        }
                    } else {
                        this.customAutocomplete();
                    }
                }
            });

        this.$scope.selectPostcode = (result: AutocompleteResult): void => {
            this.$scope.searchQuery = result.postcode;
            this.$scope.autocompleteResults = [];
            // Reset the autocomplete values so tha the results dont show anymore after you select one.
            this.$scope.validated = true;
        };

        this.$scope.validateAndSubmit = (): void => {
            if (this.$scope.validated) {
                this.$state.go("result", { postcode: this.$scope.searchQuery, region: this.$scope.region });
            }
            else {
                if (this.$scope.region !== Regions.Singapore.toString()) {
                    if (this.$scope.onlineMode) {
                        this.googleValidation((isValid) => {
                            if (isValid) {
                                this.checkPostCodeValidity(searchService);
                            } else {
                                this.$scope.autocompleteResults = [];
                                this.$scope.errorMessage = true;
                                this.$scope.$apply();
                            }

                        });
                    } else {
                        this.checkPostCodeValidity(searchService);
                    }

                }
                else {
                    if (this.dataValidation()) {
                        searchService.isValidPostcode(this.$scope.searchQuery, this.$scope.region)
                            .then((valid) => {
                                if (valid) {
                                    this.$state.go("result", { postcode: this.$scope.searchQuery, region: this.$scope.region });
                                }
                                else {
                                    this.$scope.noDataMessage = true;
                                    this.$scope.$applyAsync();
                                }
                            });
                    }
                    else {
                        this.$scope.autocompleteResults = [];
                        this.$scope.errorMessage = true;
                        this.$scope.$applyAsync();
                    }
                }
            }
        };
    }

    private googleAutocomplete = () => {
        let autocomplete = new google.maps.places.AutocompleteService();
        let places = new google.maps.places
            .PlacesService(new google.maps.Map(document.getElementById("map")));
        let request = <google.maps.places.AutocompletionRequest>{
            input: this.$scope.searchQuery,
            componentRestrictions: <google.maps.places.ComponentRestrictions>{
                country: this.$scope.region.substr(0, 2).toUpperCase()
            },
            types: ["(regions)"]
        };

        this.$scope.autocompleteResults = [];
        autocomplete.getPlacePredictions(request,
            (results, a) => {
                if (results && results.length > 0) {
                    let resultNumber = results.length;

                    results.forEach((res, index) => {
                        places.getDetails(
                            { placeId: res.place_id },
                            (place, b) => {
                                let postcode = place.address_components.filter(item => {
                                    return item.types.indexOf("postal_code") > -1;
                                })[0];

                                let state = place.address_components.filter(item => {
                                    return item.types.indexOf("administrative_area_level_1") > -1;
                                })[0];

                                if (postcode && state) {
                                    let location = <AutocompleteResult>{
                                        postcode: postcode.short_name,
                                        state: state.short_name
                                    };
                                    this.$scope.autocompleteResults.push(location);
                                }

                                if (index === resultNumber - 1) {
                                    this.$scope.isSearching = false;
                                    this.$scope.$apply();
                                }
                            });
                    });
                }
                else {
                    this.$scope.isSearching = false;
                    this.$scope.errorMessage = true;
                    this.$scope.$apply();
                }
            });
    }

    private offlineAutocomplete = (searchService: ISearchService): void => {
        searchService.autocomplete(this.$scope.searchQuery, this.$scope.region)
            .then((results) => {
                if (results) {
                    for (let result of results) {
                        let location = <AutocompleteResult>{
                            postcode: result.postcode,
                            state: result.state
                        };
                        this.$scope.autocompleteResults.push(location);
                    }
                }
                else {
                    this.$scope.noDataMessage = true;
                    this.$scope.$apply();
                }
            });
    }

    private checkPostCodeValidity = (searchService: ISearchService): void => {
        searchService.isValidPostcode(this.$scope.searchQuery, this.$scope.region)
            .then((valid) => {
                if (valid) {
                    this.$state.go("result",
                        { postcode: this.$scope.searchQuery, region: this.$scope.region });
                } else {
                    this.$scope.noDataMessage = true;
                    this.$scope.$apply();
                }
            });
    }

    private customAutocomplete = () => {
        let postcodePrefix = parseInt(this.$scope.searchQuery);

        try {

            if (postcodePrefix === 0) {
                let results = <AutocompleteResult[]>[];
                let prefix = this.$scope.searchQuery.substr(0, 1);

                while (results.length < 3) {
                    let r = Math.floor(Math.random() * (10 - 1) + 1);
                    if (results.every(item => { return item.postcode !== prefix + r; })) {
                        results.push(<AutocompleteResult>{
                            postcode: prefix + r,
                            state: ""
                        });
                    }
                }

                this.$scope.autocompleteResults = results;
            }
            else if (postcodePrefix < 9) {
                let results = <AutocompleteResult[]>[];
                let prefix = this.$scope.searchQuery.substr(0, 1);

                while (results.length < 3) {
                    let r = Math.floor(Math.random() * (10 - 0) + 0);
                    if (results.every(item => { return item.postcode !== prefix + r; })) {
                        results.push(<AutocompleteResult>{
                            postcode: prefix + r,
                            state: ""
                        });
                    }
                }

                this.$scope.autocompleteResults = results;
            }

            this.$scope.isSearching = false;
        }
        catch (error) {
            this.$scope.isSearching = false;
            this.$scope.errorMessage = true;
        }
    }

    private googleValidation = (callback: any) => {
        let autocomplete = new google.maps.places.AutocompleteService();
        let places = new google.maps.places.PlacesService(new google.maps.Map(document.getElementById("map")));
        let request = <google.maps.places.AutocompletionRequest>{
            input: this.$scope.searchQuery,
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
                                    let valid = locations.indexOf(this.$scope.searchQuery) > -1;
                                    callback(valid);
                                }
                            });

                    });
                }
                else {
                    callback(false);
                }

            });
    }

    private dataValidation = (): boolean => {
        let postcodePrefix = +this.$scope.searchQuery;
        let result = postcodePrefix && 0 < postcodePrefix && postcodePrefix < 84;

        return result;
    }
}

export { SearchController };

import { ISearchService } from "./interfaces";
import { AutocompleteResult } from "../models/autocompleteResult";

class SearchService implements ISearchService {
    public static inject = ["$http"];

    public constructor(
        private $http: ng.IHttpService,
        private $q: ng.IQService,
        private $state: ng.ui.IStateService,
        private $stateParams: ng.ui.IStateParamsService) {
    }

    public autocomplete = (query: string, region: string): ng.IPromise<AutocompleteResult[]> => {
        return this.$http.get(`api/postcode/search?query=${query}&region=${region}`)
            .then((response) => {
                return response.data;
            });
    }

    public isValidPostcode = (query: string, region: string): ng.IPromise<boolean> => {

        return this.$http.get(`api/postcode/validate?query=${query}&region=${region}`)
            .then((response) => {
                return response.data;
            });
    }
}

export { SearchService };

/**
 * Created by miked on 11/27/2015.
 */
var app = angular.module('weatheramaModule', ['ngCookies']);

app.run(['$cookies', 'apiClient', function ($cookies, apiClient) {
    var authCookie = $cookies.get('WeatheramaAccessCookie');
    if (authCookie != null)
    {
        apiClient.getUserInfo();
    }

}]);
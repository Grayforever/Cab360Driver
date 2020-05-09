using System;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Com.Google.Maps.Android;
using Java.Util;
using Newtonsoft.Json;
using Cab360Driver.EnumsConstants;
using System.Diagnostics;
using Cab360Driver.DataModels;

namespace Cab360Driver.Helpers
{
    public class MapFunctionHelper
    {
        GoogleMap mainMap;
        public Marker destinationMarker;
        public Marker positionMarker;

        bool isRequestingDirection;

        public MapFunctionHelper(GoogleMap mmap)
        {
            mainMap = mmap;
        }

        public async Task<string> GetGeoJsonAsync(string url)
        {
            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            string result = await client.GetStringAsync(url);
            return result;
        }

        public async Task<string> GetDirectionJsonAsync(LatLng location, LatLng destination)
        {
            //Origin of route
            string str_origin = "origin=" + location.Latitude + "," + location.Longitude;

            //Destination of route
            string str_destination = "destination=" + destination.Latitude + "," + destination.Longitude;

            //mode
            string mode = "mode=driving";

            //Buidling the parameters to the webservice
            string parameters = str_origin + "&" + str_destination + "&" + "&" + mode + "&key=";

            //Output format
            string output = "json";


            //Building the final url string
            string url = StringConstants.GetDirectionsBaseGateway() + output + "?" + parameters + StringConstants.GetMapKey();


            string json = "";
            json = await GetGeoJsonAsync(url);

            return json;

        }

        public void DrawTripOnMap(string json)
        {
            Android.Gms.Maps.Model.Polyline mPolyline;
            Marker pickupMarker;

            var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);

            var pointCode = directionData.routes[0].overview_polyline.points;
            var line = PolyUtil.Decode(pointCode);
            LatLng firstpoint = line[0];
            LatLng lastpoint = line[line.Count - 1];

            //My take off position
            MarkerOptions markerOptions = new MarkerOptions();
            markerOptions.SetPosition(firstpoint);
            markerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));
            pickupMarker = mainMap.AddMarker(markerOptions);

            //Constanly Changing Current Location;
            MarkerOptions positionMarkerOption = new MarkerOptions();
            positionMarkerOption.SetPosition(firstpoint);
            positionMarkerOption.SetTitle("Current Location");
            positionMarkerOption.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.position));
            positionMarker = mainMap.AddMarker(positionMarkerOption);

            MarkerOptions markerOptions1 = new MarkerOptions();
            markerOptions1.SetPosition(lastpoint);
            markerOptions1.SetTitle("Pickup Location");
            markerOptions1.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));
            destinationMarker = mainMap.AddMarker(markerOptions1);

            ArrayList routeList = new ArrayList();
            int locationCount = 0;
            foreach(LatLng item in line)
            {
                routeList.Add(item);
                locationCount++;
                Debug.WriteLine("Position " + locationCount.ToString() + " = " + item.Latitude.ToString() + " , " + item.Longitude.ToString());

            }

            PolylineOptions polylineOptions = new PolylineOptions()
                .AddAll(routeList)
                .InvokeWidth(20)
                .InvokeColor(Color.Teal)
                .InvokeStartCap(new SquareCap())
                .InvokeEndCap(new SquareCap())
                .InvokeJointType(JointType.Round)
                .Geodesic(true);

            mPolyline = mainMap.AddPolyline(polylineOptions);
            mainMap.UiSettings.ZoomControlsEnabled = true;
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(firstpoint, 15));
            destinationMarker.ShowInfoWindow();


        }


        public void DrawTripToDestination(string json)
        {
            Android.Gms.Maps.Model.Polyline mPolyline;
            Marker pickupMarker;

            var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);

            var pointCode = directionData.routes[0].overview_polyline.points;
            var line = PolyUtil.Decode(pointCode);
            LatLng firstpoint = line[0];
            LatLng lastpoint = line[line.Count - 1];

            //My take off position
            MarkerOptions markerOptions = new MarkerOptions();
            markerOptions.SetPosition(firstpoint);
            markerOptions.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueGreen));
            markerOptions.SetTitle("Pickup Location");
            pickupMarker = mainMap.AddMarker(markerOptions);

            //Constanly Changing Current Location;
            MarkerOptions positionMarkerOption = new MarkerOptions();
            positionMarkerOption.SetPosition(firstpoint);
            positionMarkerOption.SetTitle("Current Location");
            positionMarkerOption.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.position));
            positionMarker = mainMap.AddMarker(positionMarkerOption);

            MarkerOptions markerOptions1 = new MarkerOptions();
            markerOptions1.SetPosition(lastpoint);
            markerOptions1.SetTitle("Destination");
            markerOptions1.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));
            destinationMarker = mainMap.AddMarker(markerOptions1);

            ArrayList routeList = new ArrayList();
            int locationCount = 0;
            foreach (LatLng item in line)
            {
                routeList.Add(item);
                locationCount++;
                Debug.WriteLine("Position " + locationCount.ToString() + " = " + item.Latitude.ToString() + " , " + item.Longitude.ToString());

            }

            PolylineOptions polylineOptions = new PolylineOptions()
                .AddAll(routeList)
                .InvokeWidth(20)
                .InvokeColor(Color.Teal)
                .InvokeStartCap(new SquareCap())
                .InvokeEndCap(new SquareCap())
                .InvokeJointType(JointType.Round)
                .Geodesic(true);

            mPolyline = mainMap.AddPolyline(polylineOptions);
            mainMap.UiSettings.ZoomControlsEnabled = true;
            mainMap.TrafficEnabled = true;

            LatLng southwest = new LatLng(directionData.routes[0].bounds.southwest.lat, directionData.routes[0].bounds.southwest.lng);
            LatLng northeast = new LatLng(directionData.routes[0].bounds.northeast.lat, directionData.routes[0].bounds.northeast.lng);

            LatLngBounds tripBounds = new LatLngBounds(southwest, northeast);
            mainMap.AnimateCamera(CameraUpdateFactory.NewLatLngBounds(tripBounds, 100));
            destinationMarker.ShowInfoWindow();

        }

        public async void UpdateMovement(LatLng myposition, LatLng destination, string whereto)
        {
            positionMarker.Visible = true;
            positionMarker.Position = myposition;

            if (!isRequestingDirection)
            {
                isRequestingDirection = true;
                string json = await GetDirectionJsonAsync(myposition, destination);
                var directionData = JsonConvert.DeserializeObject<DirectionParser>(json);
                string duration = directionData.routes[0].legs[0].duration.text;
                positionMarker.Title = "Current Location";
                positionMarker.Snippet = duration + "Away from " + whereto;
                positionMarker.ShowInfoWindow();
                isRequestingDirection = false;
            }
          
        }

        public async Task<double> CalculateFares(LatLng firstpoint, LatLng lastpoint)
        {
            string directionJson = await GetDirectionJsonAsync(firstpoint, lastpoint);
            var directionData = JsonConvert.DeserializeObject<DirectionParser>(directionJson);

            double distanceValue = directionData.routes[0].legs[0].distance.value;
            double durationValue = directionData.routes[0].legs[0].duration.value;

            double basefare = 10; //usd
            double distanceFare = 5;
            double timeFare = 5;

            double kmfare = (distanceValue / 1000) * distanceFare;
            double minsFares = (durationValue / 60) * timeFare;
            double amount = basefare + kmfare + minsFares;
            double fare = Math.Floor(amount / 10) * 10;
            return fare;
        }
    }
}

﻿using Router.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using Router.Proxies;
using Nominatim.API.Models;
using GeoJSON.Net.Geometry;
using Router.Utils;


namespace Router.APIHelpers
{
    public static class OsrmAPIHelper
    {
        public static OsrmJsonRouteModel GetSimpleRoute(params Position[] coordinates)
        {
            string positionsString = StringUtils.GetStringFromPositions(coordinates);

            string uri = $"http://router.project-osrm.org/route/v1/driving/{positionsString}?geometries=geojson&overview=full";         
            string html = HttpProxy.DownloadResource(uri);
            OsrmJsonRouteModel parsed = JsonConvert.DeserializeObject<OsrmJsonRouteModel>(html);
            return parsed;
        }
        
        public static OsrmJsonRouteModel GetSimpleRoute(string sourceQuery, string destinationQuery, params Position[] intermediates)
        {
            Position source = NominatimAPIHelper.GetPositionForAddress(sourceQuery);
            Position destination = NominatimAPIHelper.GetPositionForAddress(destinationQuery);
            Position[] positions = GetPositionsArray(source, destination, intermediates);
            return GetSimpleRoute(positions);
        }

        public static OsrmJsonRouteModel GetOptimalRoute(Position first, Position last, params Position[] intermediates)
        {
            Position[] positions = GetPositionsArray(first, last, intermediates);
            string positionsString = StringUtils.GetStringFromPositions(positions);
            string uri = $"http://router.project-osrm.org/trip/v1/driving/{positionsString}?roundtrip=false&source=first&destination=last&geometries=geojson&overview=full";
            string html = HttpProxy.DownloadResource(uri);
            OsrmJsonRouteModel parsed = JsonConvert.DeserializeObject<OsrmJsonRouteModel>(html);
            return parsed;
        }

        public static OsrmJsonRouteModel GetOptimalRoute(string sourceQuery, string destinationQuery, params Position[] intermediates)
        {
            Position sourceCoordinate = NominatimAPIHelper.GetPositionForAddress(sourceQuery);
            Position destinationCoordinate = NominatimAPIHelper.GetPositionForAddress(destinationQuery);
            return GetOptimalRoute(sourceCoordinate, destinationCoordinate, intermediates);
        }

        public static TravelTimesMatrixModel GetTravelTimesMatrix(Position source, params Position[] desinations)
        {
            Position[] positions = new Position[desinations.Length + 1];
            positions[0] = source;
            Array.Copy(desinations, 0, positions, 1, desinations.Length);
            string positionString = StringUtils.GetStringFromPositions(positions);

            string uri = $"http://router.project-osrm.org/table/v1/driving/{positionString}?sources=0";
            string json = HttpProxy.DownloadResource(uri);
            TravelTimesMatrixModel parsed = JsonConvert.DeserializeObject<TravelTimesMatrixModel>(json);
            return parsed;
        }
                

        private static Position[] GetPositionsArray(Position source, Position destination, Position[] intermediates)
        {
            Position[] positions = new Position[intermediates.Length + 2];
            positions[0] = source;
            Array.Copy(intermediates, 0, positions, 1, intermediates.Length);
            positions[positions.Length - 1] = destination;
            return positions;
        }

        

    }
}
import React, { useState, useEffect } from 'react';
import './custom.css';
import 'leaflet/dist/leaflet.css'; // Import Leaflet CSS
import L from 'leaflet';
import logo from './images/LogoWoda.png';
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import MarkerClusterGroup from 'react-leaflet-cluster';
import { Icon, divIcon, point } from "leaflet";
import zielony from './images/zielony.png'; 
import zolty from './images/zolty.png';   
import czerwony from './images/czerwony.png';



const App = () => {
    const [measurements, setMeasurements] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const apiUrl = 'https://localhost:7204/StationInfo/latest'; // API endpoint

    useEffect(() => {
        const fetchData = async () => {
            try {
                const response = await fetch(apiUrl);
                if (!response.ok) {
                    throw new Error(`API request failed with status ${response.status}`);
                }
                const data = await response.json();
                console.log(data);
                setMeasurements(data);
            } catch (error) {
                console.error('Error fetching data:', error);
                setError(error.message);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, []);


    const renderMeasurementsTable = () => {
        return (
            <table className="table table-striped" aria-labelledby="tableLabel">
                <thead>
                    <tr>
                        <th>River Name</th>
                        <th>City</th>
                        <th>Date / Time</th>
                        <th>Water Level</th>
                        <th>Warning Level</th>
                        <th>Alarm Level</th>
                    </tr>
                </thead>
                <tbody>
                    {measurements.map((measurement, index) => (
                        <tr key={index}>
                            <td>{measurement.riverName}</td>
                            <td>{measurement.city}</td>
                            <td>{measurement.measurementDateTime}</td>
                            <td
                                style={{
                                    backgroundColor: measurement.measurementWaterLevel >
                                        measurement.alarmLevel
                                        ? 'red'
                                        : measurement.measurementWaterLevel > measurement.warningLevel
                                            ? 'yellow'
                                            : 'green',
                                }}
                            >
                                {measurement.measurementWaterLevel}
                            </td>
                            <td>{measurement.warningLevel}</td>
                            <td>{measurement.alarmLevel}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        );
    };

    const initMap = () => {
        const map = L.map('map').setView([50.041232, 21.998335], 8);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(map);

        if (measurements.length > 0) {
            measurements.forEach((measurement, index) => {
                if (measurement.hasOwnProperty('latitude') && measurement.hasOwnProperty('longitude')) {
                    const markerColor = measurement.measurementWaterLevel > measurement.alarmLevel ? 'red' :
                        measurement.measurementWaterLevel > measurement.warningLevel ? 'yellow' : 'green';

                    const marker = L.marker([measurement.latitude, measurement.longitude], {
                        icon: L.icon({ iconUrl: zielony, iconSize: [25, 41] }),
                        icon: divIcon({
                            className: 'marker-icon', // Add a CSS class for styling
                            html: `<div style="background-color: ${markerColor}; width: 15px; height: 15px; border-radius: 50%;"></div>`,
                        })
                    });
                    marker.addTo(map);

                    // Optional: Add a popup (consider performance for large datasets)
                    marker.bindPopup(`
                    <b>River Name:</b> ${measurement.riverName}<br>
                    <b>City:</b> ${measurement.city}<br>
                    <b>Water Level:</b> ${measurement.measurementWaterLevel}<br>
                    <b>Warning Level:</b> ${measurement.warningLevel}<br>
                    <b>Alarm Level:</b> ${measurement.alarmLevel}
                `);
                } else {
                    console.warn(`Measurement ${index} is missing latitude or longitude. Marker not created.`);
                }
            });
        } else {
            console.info('No measurements loaded yet. Markers will be created when data becomes available.');
        }
    };


    return (
        <div class="main">
            <div className="head">
                <img src={logo} alt="Logo" ></img>
            <p>Podkarpacki Instytut Wodny</p>
            </div>
            <div className="container">
                <div className="row">
                    {/* Map container (50% width) */}
                    <div className="col-md-6">
                        <div id="map" style={{ height: '400px' }} />
                        {measurements.length > 0 && initMap()} {/* Initialize map on window load */}
                    </div>
                    {/* Table container (50% width) */}
                    <div className="col-md-6">
                        <div>
                            <h1 id="tableLabel">Latest Water Measurements</h1>
                            <p>Real-time reading of water levels in rivers</p>
                            {loading && <p><em>Loading...</em></p>}
                            {error && <p className="error">Error: {error}</p>}
                            {measurements.length > 0 && renderMeasurementsTable()}
                        </div>
                    </div>
                </div>
            </div>
        </div>

    );
};

export default App;

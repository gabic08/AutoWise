window.addEventListener("DOMContentLoaded", async () => {
    const isAuthenticated = await initAuth();

    if (!isAuthenticated) {
        window.location.href = "index.html";
        return;
    }

    const params = new URLSearchParams(window.location.search);
    const vehicleId = params.get("id");

    if (!vehicleId) {
        window.location.href = "index.html";
        return;
    }

    const vehicle = await getUserVehicleById(vehicleId);

    document.getElementById("detail-title").textContent = `${vehicle.year ?? ""} ${vehicle.make ?? ""} ${vehicle.model ?? ""}`;
    document.getElementById("detail-plate").textContent = `License plate: ${vehicle.licensePlateNumber}`;
    document.getElementById("detail-vin").textContent = `VIN: ${vehicle.vin}`;
});

document.getElementById("logout-button").addEventListener("click", logout);
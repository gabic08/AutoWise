window.addEventListener("DOMContentLoaded", async () => {
    const isAuthenticated = await initAuth();

    if (!isAuthenticated){
        showLoginView();
        return;
    }

    showAppView();
    await loadVehicleList();
})

document.getElementById("login-button").addEventListener("click", login);
document.getElementById("logout-button").addEventListener("click", logout);

document.getElementById("add-vehicle-form").addEventListener("submit", async (event) =>{
    event.preventDefault();

    const vin = document.getElementById("vin-input").value;
    const licensePlateNumber = document.getElementById("plate-input").value;

    await createUserVehicle(vin, licensePlateNumber);
    event.target.reset();
    await loadVehicleList();
});

function showLoginView(){
    document.getElementById("login-view").classList.remove("hidden");
    document.getElementById("app-view").classList.add("hidden");
    document.getElementById("logout-button").classList.add("hidden");
}

function showAppView() {
  document.getElementById("login-view").classList.add("hidden");
  document.getElementById("app-view").classList.remove("hidden");
  document.getElementById("logout-button").classList.remove("hidden");
}


async function loadVehicleList(){
    const result = await getUserVehicles();
    renderVehicleList(result.entities);
}

function renderVehicleList(vehicles){
    const container = document.getElementById("vehicle-list");
    container.innerHTML = "";

    for (const vehicle of vehicles){
        const card = document.createElement("div");
        card.className = "vehicle-card";
        card.textContent = `${vehicle.year ?? ""} ${vehicle.make ?? ""} ${vehicle.model ?? ""} - ${vehicle.licensePlateNumber}`;
        card.addEventListener("click", () => {
            window.location.href = `vehicle-detail.html?id=${vehicle.id}`;
        });
        container.appendChild(card);
    }
}

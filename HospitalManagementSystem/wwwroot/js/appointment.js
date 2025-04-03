$(document).ready(function () {

    // Event listener for doctor selection change
    $("#DoctorId").change(function () {
        var doctorId = $(this).val();

        // Clear previous slots
        $("#AvailableSlots").empty().append("<option value=''>Loading slots...</option>");

        if (doctorId) {
            $.ajax({
                url: `/Doctor/GetAvailableSlots?doctorId=${doctorId}`,
                type: "GET",
                dataType: "json",
                success: function (data) {
                    if (data && data.length > 0) {
                        var options = "<option value=''>-- Select Slot --</option>";
                        $.each(data, function (index, slot) {
                            options += `<option value="${slot.startTime}">${slot.startTime} - ${slot.endTime}</option>`;
                        });
                        $("#AvailableSlots").html(options);
                    } else {
                        $("#AvailableSlots").html("<option value=''>No slots available</option>");
                    }
                },
                error: function (xhr, status, error) {
                    console.error("Error loading slots:", error);
                    $("#AvailableSlots").html("<option value=''>Failed to load slots</option>");
                }
            });
        } else {
            $("#AvailableSlots").html("<option value=''>-- Select Slot --</option>");
        }
    });
});

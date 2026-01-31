using ORM.Migrations;
using ORM.Models;
using System.Reflection;

namespace ORM;
public class MedicalApp
{
    private readonly string _connectionString;

    public MedicalApp(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Run()
    {
        Console.WriteLine("=== Medical System ===\n");

        var runner = new MigrationRunner(_connectionString);
        runner.MigrateUp(Assembly.GetExecutingAssembly());

        using var context = new MedicalContext(_connectionString);

        if (context.Doctors.ToList().Count == 0)
        {
            InitializeDoctors(context);
        }

        if (context.Medications.ToList().Count == 0)
        {
            InitializeMedications(context);
        }

        while (true)
        {
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Patients");
            Console.WriteLine("2. Medical History");
            Console.WriteLine("3. Prescriptions");
            Console.WriteLine("4. Appointments");
            Console.WriteLine("5. View Doctors");
            Console.WriteLine("0. Exit");
            Console.Write("\nChoice: ");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1": PatientMenu(context); break;
                case "2": MedicalHistoryMenu(context); break;
                case "3": PrescriptionMenu(context); break;
                case "4": AppointmentMenu(context); break;
                case "5": ViewDoctors(context); break;
                case "0": return;
            }
        }
    }

    private void InitializeDoctors(MedicalContext context)
    {
        Console.WriteLine("\n--- First Run: Initialize Doctors ---\n");

        var doctors = new[]
        {
            new Doctor { FirstName = "Ivan", LastName = "Horvat", Specialization = "CT" },
            new Doctor { FirstName = "Ana", LastName = "Kovač", Specialization = "MR" },
            new Doctor { FirstName = "Marko", LastName = "Novak", Specialization = "ULTRA" },
            new Doctor { FirstName = "Petra", LastName = "Babić", Specialization = "EKG" },
            new Doctor { FirstName = "Luka", LastName = "Jurić", Specialization = "ECHO" },
            new Doctor { FirstName = "Maja", LastName = "Knežević", Specialization = "OKO" },
            new Doctor { FirstName = "Tomislav", LastName = "Vuković", Specialization = "DERM" },
            new Doctor { FirstName = "Ivana", LastName = "Šimić", Specialization = "DENTA" },
            new Doctor { FirstName = "Josip", LastName = "Matić", Specialization = "MAMMO" },
            new Doctor { FirstName = "Katarina", LastName = "Perić", Specialization = "EEG" }
        };

        foreach (var doctor in doctors)
        {
            context.Doctors.Add(doctor);
            Console.WriteLine($"Added: Dr. {doctor.FirstName} {doctor.LastName} ({doctor.Specialization})");
        }

        Console.WriteLine("\nDoctors initialized successfully.");
    }

    private void InitializeMedications(MedicalContext context)
    {
        var medications = new[]
        {
            new Medication { Name = "Ibuprofen", DoseUnit = "mg" },
            new Medication { Name = "Paracetamol", DoseUnit = "mg" },
            new Medication { Name = "Amoxicillin", DoseUnit = "mg" },
            new Medication { Name = "Metformin", DoseUnit = "mg" },
            new Medication { Name = "Aspirin", DoseUnit = "mg" },
            new Medication { Name = "Insulin", DoseUnit = "units" },
            new Medication { Name = "Omeprazole", DoseUnit = "mg" },
            new Medication { Name = "Lisinopril", DoseUnit = "mg" }
        };

        foreach (var med in medications)
        {
            context.Medications.Add(med);
        }
    }

    private void ViewDoctors(MedicalContext context)
    {
        Console.WriteLine("\n--- Doctors ---");
        var doctors = context.Doctors.ToList();
        foreach (var d in doctors)
        {
            Console.WriteLine($"[{d.Id}] Dr. {d.FirstName} {d.LastName} - {d.Specialization}");
        }
    }

    private void PatientMenu(MedicalContext context)
    {
        while (true)
        {
            Console.WriteLine("\n--- Patients ---");
            Console.WriteLine("1. List all");
            Console.WriteLine("2. Add new");
            Console.WriteLine("3. Update");
            Console.WriteLine("4. Delete");
            Console.WriteLine("5. View details");
            Console.WriteLine("0. Back");
            Console.Write("\nChoice: ");

            switch (Console.ReadLine())
            {
                case "1":
                    var patients = context.Patients.ToList();
                    foreach (var p in patients)
                        Console.WriteLine($"[{p.Id}] {p.FirstName} {p.LastName} (OIB: {p.OIB})");
                    break;

                case "2":
                    var newPatient = new Patient();
                    Console.Write("First name: "); newPatient.FirstName = Console.ReadLine() ?? "";
                    Console.Write("Last name: "); newPatient.LastName = Console.ReadLine() ?? "";
                    Console.Write("OIB (11 digits): "); newPatient.OIB = Console.ReadLine() ?? "";
                    Console.Write("Birth date (yyyy-MM-dd): ");
                    DateTime.TryParse(Console.ReadLine(), out var bd); newPatient.BirthDate = bd;
                    Console.Write("Gender (M/F): "); newPatient.Gender = Console.ReadLine() ?? "";
                    Console.Write("Residence address: "); newPatient.ResidenceAddress = Console.ReadLine() ?? "";
                    Console.Write("Domicile address: "); newPatient.DomicileAddress = Console.ReadLine() ?? "";
                    context.Patients.Add(newPatient);
                    Console.WriteLine($"Patient added with ID: {newPatient.Id}");
                    break;

                case "3":
                    Console.Write("Patient ID to update: ");
                    if (int.TryParse(Console.ReadLine(), out var updateId))
                    {
                        var patient = context.Patients.Find(updateId);
                        if (patient != null)
                        {
                            Console.Write($"First name [{patient.FirstName}]: ");
                            var fn = Console.ReadLine(); if (!string.IsNullOrEmpty(fn)) patient.FirstName = fn;
                            Console.Write($"Last name [{patient.LastName}]: ");
                            var ln = Console.ReadLine(); if (!string.IsNullOrEmpty(ln)) patient.LastName = ln;
                            Console.Write($"Residence address [{patient.ResidenceAddress}]: ");
                            var ra = Console.ReadLine(); if (!string.IsNullOrEmpty(ra)) patient.ResidenceAddress = ra;
                            Console.Write($"Domicile address [{patient.DomicileAddress}]: ");
                            var da = Console.ReadLine(); if (!string.IsNullOrEmpty(da)) patient.DomicileAddress = da;
                            context.Patients.Update(patient);
                            Console.WriteLine("Patient updated.");
                        }
                    }
                    break;

                case "4":
                    Console.Write("Patient ID to delete: ");
                    if (int.TryParse(Console.ReadLine(), out var deleteId))
                    {
                        context.Patients.Delete(deleteId);
                        Console.WriteLine("Patient deleted.");
                    }
                    break;

                case "5":
                    Console.Write("Patient ID: ");
                    if (int.TryParse(Console.ReadLine(), out var viewId))
                    {
                        var patient = context.Patients.Find(viewId);
                        if (patient != null)
                        {
                            Console.WriteLine($"\n{patient.FirstName} {patient.LastName}");
                            Console.WriteLine($"OIB: {patient.OIB}");
                            Console.WriteLine($"Birth date: {patient.BirthDate:yyyy-MM-dd}");
                            Console.WriteLine($"Gender: {patient.Gender}");
                            Console.WriteLine($"Residence: {patient.ResidenceAddress}");
                            Console.WriteLine($"Domicile: {patient.DomicileAddress}");

                            Console.WriteLine("\nMedical History (lazy loaded):");
                            var histories = patient.MedicalHistories?.Items ?? new List<MedicalHistory>();
                            foreach (var h in histories)
                                Console.WriteLine($"  - {h.DiseaseName} ({h.StartDate:yyyy-MM-dd} to {h.EndDate?.ToString("yyyy-MM-dd") ?? "ongoing"})");

                            Console.WriteLine("\nPrescriptions (lazy loaded):");
                            var prescriptions = patient.Prescriptions?.Items ?? new List<Prescription>();
                            foreach (var pr in prescriptions)
                            {
                                var med = pr.Medication?.Value;
                                Console.WriteLine($"  - {med?.Name ?? "Unknown"} {pr.DoseAmount} {med?.DoseUnit ?? ""}, {pr.Frequency} for {pr.Condition}");
                            }

                            Console.WriteLine("\nAppointments (lazy loaded):");
                            var appointments = patient.Appointments?.Items ?? new List<Appointment>();
                            foreach (var a in appointments)
                            {
                                var doc = a.Doctor?.Value;
                                Console.WriteLine($"  - {a.ExaminationType} on {a.ScheduledAt:yyyy-MM-dd HH:mm} with Dr. {doc?.LastName ?? "Unknown"}");
                            }
                        }
                    }
                    break;

                case "0": return;
            }
        }
    }

    private void MedicalHistoryMenu(MedicalContext context)
    {
        while (true)
        {
            Console.WriteLine("\n--- Medical History ---");
            Console.WriteLine("1. List all");
            Console.WriteLine("2. Add new");
            Console.WriteLine("3. Update (mark as ended)");
            Console.WriteLine("4. Delete");
            Console.WriteLine("0. Back");
            Console.Write("\nChoice: ");

            switch (Console.ReadLine())
            {
                case "1":
                    var histories = context.MedicalHistories.ToList();
                    foreach (var h in histories)
                    {
                        var patient = h.Patient?.Value;
                        Console.WriteLine($"[{h.Id}] {patient?.FirstName} {patient?.LastName}: {h.DiseaseName} ({h.StartDate:yyyy-MM-dd} to {h.EndDate?.ToString("yyyy-MM-dd") ?? "ongoing"})");
                    }
                    break;

                case "2":
                    var newHistory = new MedicalHistory();
                    Console.Write("Patient ID: ");
                    int.TryParse(Console.ReadLine(), out var patientId); newHistory.PatientId = patientId;
                    Console.Write("Disease name: "); newHistory.DiseaseName = Console.ReadLine() ?? "";
                    Console.Write("Start date (yyyy-MM-dd): ");
                    DateTime.TryParse(Console.ReadLine(), out var startDate); newHistory.StartDate = startDate;
                    Console.Write("End date (yyyy-MM-dd, leave empty if ongoing): ");
                    var endStr = Console.ReadLine();
                    if (!string.IsNullOrEmpty(endStr) && DateTime.TryParse(endStr, out var endDate))
                        newHistory.EndDate = endDate;
                    context.MedicalHistories.Add(newHistory);
                    Console.WriteLine($"Medical history added with ID: {newHistory.Id}");
                    break;

                case "3":
                    Console.Write("History ID to update: ");
                    if (int.TryParse(Console.ReadLine(), out var updateId))
                    {
                        var history = context.MedicalHistories.Find(updateId);
                        if (history != null)
                        {
                            Console.Write("End date (yyyy-MM-dd): ");
                            if (DateTime.TryParse(Console.ReadLine(), out var ed))
                            {
                                history.EndDate = ed;
                                context.MedicalHistories.Update(history);
                                Console.WriteLine("Medical history updated.");
                            }
                        }
                    }
                    break;

                case "4":
                    Console.Write("History ID to delete: ");
                    if (int.TryParse(Console.ReadLine(), out var deleteId))
                    {
                        context.MedicalHistories.Delete(deleteId);
                        Console.WriteLine("Medical history deleted.");
                    }
                    break;

                case "0": return;
            }
        }
    }

    private void PrescriptionMenu(MedicalContext context)
    {
        while (true)
        {
            Console.WriteLine("\n--- Prescriptions ---");
            Console.WriteLine("1. List all");
            Console.WriteLine("2. Add new");
            Console.WriteLine("3. Update");
            Console.WriteLine("4. Delete");
            Console.WriteLine("5. View medications");
            Console.WriteLine("0. Back");
            Console.Write("\nChoice: ");

            switch (Console.ReadLine())
            {
                case "1":
                    var prescriptions = context.Prescriptions.ToList();
                    foreach (var pr in prescriptions)
                    {
                        var patient = pr.Patient?.Value;
                        var med = pr.Medication?.Value;
                        Console.WriteLine($"[{pr.Id}] {patient?.FirstName} {patient?.LastName}: {med?.Name} {pr.DoseAmount}{med?.DoseUnit}, {pr.Frequency}");
                    }
                    break;

                case "2":
                    var newPrescription = new Prescription();
                    Console.Write("Patient ID: ");
                    int.TryParse(Console.ReadLine(), out var patientId); newPrescription.PatientId = patientId;
                    Console.Write("Medication ID: ");
                    int.TryParse(Console.ReadLine(), out var medId); newPrescription.MedicationId = medId;
                    Console.Write("Condition: "); newPrescription.Condition = Console.ReadLine() ?? "";
                    Console.Write("Dose amount: ");
                    decimal.TryParse(Console.ReadLine(), out var dose); newPrescription.DoseAmount = dose;
                    Console.Write("Frequency (e.g., '3x daily', 'every 8 hours'): "); newPrescription.Frequency = Console.ReadLine() ?? "";
                    newPrescription.StartDate = DateTime.Now;
                    context.Prescriptions.Add(newPrescription);
                    Console.WriteLine($"Prescription added with ID: {newPrescription.Id}");
                    break;

                case "3":
                    Console.Write("Prescription ID to update: ");
                    if (int.TryParse(Console.ReadLine(), out var updateId))
                    {
                        var prescription = context.Prescriptions.Find(updateId);
                        if (prescription != null)
                        {
                            Console.Write($"Dose amount [{prescription.DoseAmount}]: ");
                            var doseStr = Console.ReadLine();
                            if (!string.IsNullOrEmpty(doseStr) && decimal.TryParse(doseStr, out var newDose))
                                prescription.DoseAmount = newDose;
                            Console.Write($"Frequency [{prescription.Frequency}]: ");
                            var freq = Console.ReadLine();
                            if (!string.IsNullOrEmpty(freq)) prescription.Frequency = freq;
                            Console.Write("End date (yyyy-MM-dd, leave empty to keep active): ");
                            var endStr = Console.ReadLine();
                            if (!string.IsNullOrEmpty(endStr) && DateTime.TryParse(endStr, out var ed))
                                prescription.EndDate = ed;
                            context.Prescriptions.Update(prescription);
                            Console.WriteLine("Prescription updated.");
                        }
                    }
                    break;

                case "4":
                    Console.Write("Prescription ID to delete: ");
                    if (int.TryParse(Console.ReadLine(), out var deleteId))
                    {
                        context.Prescriptions.Delete(deleteId);
                        Console.WriteLine("Prescription deleted.");
                    }
                    break;

                case "5":
                    var medications = context.Medications.ToList();
                    foreach (var m in medications)
                        Console.WriteLine($"[{m.Id}] {m.Name} ({m.DoseUnit})");
                    break;

                case "0": return;
            }
        }
    }

    private void AppointmentMenu(MedicalContext context)
    {
        while (true)
        {
            Console.WriteLine("\n--- Appointments ---");
            Console.WriteLine("1. List all");
            Console.WriteLine("2. Schedule new");
            Console.WriteLine("3. Update");
            Console.WriteLine("4. Cancel (delete)");
            Console.WriteLine("0. Back");
            Console.Write("\nChoice: ");

            switch (Console.ReadLine())
            {
                case "1":
                    var appointments = context.Appointments.ToList();
                    foreach (var a in appointments)
                    {
                        var patient = a.Patient?.Value;
                        var doctor = a.Doctor?.Value;
                        Console.WriteLine($"[{a.Id}] {patient?.FirstName} {patient?.LastName} - {a.ExaminationType} on {a.ScheduledAt:yyyy-MM-dd HH:mm} with Dr. {doctor?.LastName}");
                    }
                    break;

                case "2":
                    var newAppointment = new Appointment();
                    Console.Write("Patient ID: ");
                    int.TryParse(Console.ReadLine(), out var patientId); newAppointment.PatientId = patientId;

                    Console.WriteLine("Examination types: CT, MR, ULTRA, EKG, ECHO, OKO, DERM, DENTA, MAMMO, EEG");
                    Console.Write("Examination type: "); newAppointment.ExaminationType = Console.ReadLine()?.ToUpper() ?? "";

                    var doctors = context.Doctors.ToList()
                        .Where(d => d.Specialization == newAppointment.ExaminationType).ToList();
                    if (doctors.Any())
                    {
                        Console.WriteLine("Available doctors:");
                        foreach (var d in doctors)
                            Console.WriteLine($"  [{d.Id}] Dr. {d.FirstName} {d.LastName}");
                    }

                    Console.Write("Doctor ID: ");
                    int.TryParse(Console.ReadLine(), out var doctorId); newAppointment.DoctorId = doctorId;

                    Console.Write("Date and time (yyyy-MM-dd HH:mm): ");
                    DateTime.TryParse(Console.ReadLine(), out var scheduledAt); newAppointment.ScheduledAt = scheduledAt;

                    Console.Write("Notes: "); newAppointment.Notes = Console.ReadLine() ?? "";

                    context.Appointments.Add(newAppointment);
                    Console.WriteLine($"Appointment scheduled with ID: {newAppointment.Id}");
                    break;

                case "3":
                    Console.Write("Appointment ID to update: ");
                    if (int.TryParse(Console.ReadLine(), out var updateId))
                    {
                        var appointment = context.Appointments.Find(updateId);
                        if (appointment != null)
                        {
                            Console.Write($"New date/time [{appointment.ScheduledAt:yyyy-MM-dd HH:mm}]: ");
                            var dtStr = Console.ReadLine();
                            if (!string.IsNullOrEmpty(dtStr) && DateTime.TryParse(dtStr, out var newDt))
                                appointment.ScheduledAt = newDt;
                            Console.Write($"Notes [{appointment.Notes}]: ");
                            var notes = Console.ReadLine();
                            if (!string.IsNullOrEmpty(notes)) appointment.Notes = notes;
                            context.Appointments.Update(appointment);
                            Console.WriteLine("Appointment updated.");
                        }
                    }
                    break;

                case "4":
                    Console.Write("Appointment ID to cancel: ");
                    if (int.TryParse(Console.ReadLine(), out var deleteId))
                    {
                        context.Appointments.Delete(deleteId);
                        Console.WriteLine("Appointment cancelled.");
                    }
                    break;

                case "0": return;
            }
        }
    }
}

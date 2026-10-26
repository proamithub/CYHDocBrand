using System.Text.Json;
using CYHDocBrandModel.DTO;

namespace CYHDocBrandData;

public class TemplateSeedCategory
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-folder-fill";
    public string Color { get; set; } = "#00f2fe";
    public int Order { get; set; }
}

public class TemplateSeedItem
{
    public string CategorySlug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string StyleTheme { get; set; } = "Modern 3D";
    public string AspectRatio { get; set; } = "1:1";
    public string ImageUrl { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = "#00f2fe";
    public string SecondaryColor { get; set; } = "#0a192f";
    public string Heading { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string BadgeText { get; set; } = string.Empty;
    public string Language { get; set; } = "en"; // "en" or "hi"
    public int Order { get; set; }
}

public static class TemplateSeedData
{
    public static List<TemplateSeedCategory> GetCategories() => new()
    {
        new() { Name = "Cardiology & Heart Care", Slug = "cardiology", Icon = "bi-heart-pulse-fill", Color = "#ef4444", Order = 1 },
        new() { Name = "Dental & Oral Health", Slug = "dental", Icon = "bi-emoji-smile-fill", Color = "#06b6d4", Order = 2 },
        new() { Name = "Orthopedics & Spine", Slug = "orthopedics", Icon = "bi-person-walking", Color = "#3b82f6", Order = 3 },
        new() { Name = "Pediatrics & Child Care", Slug = "pediatrics", Icon = "bi-balloon-fill", Color = "#f59e0b", Order = 4 },
        new() { Name = "Gynecology & Maternity", Slug = "gynecology", Icon = "bi-gender-female", Color = "#ec4899", Order = 5 },
        new() { Name = "General Health & Diabetes", Slug = "general-health", Icon = "bi-shield-plus", Color = "#10b981", Order = 6 },
        new() { Name = "Eye, Skin & ENT Care", Slug = "eye-skin-ent", Icon = "bi-eye-fill", Color = "#8b5cf6", Order = 7 },
        new() { Name = "24x7 Emergency & ICU", Slug = "emergency", Icon = "bi-hospital-fill", Color = "#dc2626", Order = 8 },
        new() { Name = "Festivals & Health Greetings", Slug = "festivals", Icon = "bi-stars", Color = "#facc15", Order = 9 },
        new() { Name = "Clinic & Doctor Announcements", Slug = "announcements", Icon = "bi-megaphone-fill", Color = "#0ea5e9", Order = 10 },
    };

    public static List<TemplateSeedItem> GetTemplates() => new()
    {
        // Category 1: Cardiology (1-5)
        new() {
            CategorySlug = "cardiology", Order = 1, ImageUrl = "images/template/template_01.svg",
            Title = "विश्व हृदय दिवस: स्वस्थ दिल, खुशहाल जीवन", StyleTheme = "Neon Glow",
            PrimaryColor = "#00f2fe", SecondaryColor = "#091224",
            BadgeText = "हृदय स्वास्थ्य जागरूकता", Heading = "विश्व हृदय दिवस: दिल का रखें खास ख्याल",
            Subtitle = "हर धड़कन अनमोल है। नियमित प्रिवेंटिव कार्डियक चेकअप से अपने दिल को स्वस्थ और सुरक्षित रखें।",
            Language = "hi"
        },
        new() {
            CategorySlug = "cardiology", Order = 2, ImageUrl = "images/template/template_02.svg",
            Title = "Hypertension & Blood Pressure Care", StyleTheme = "Dark Tech",
            PrimaryColor = "#38bdf8", SecondaryColor = "#0b162c",
            BadgeText = "BP MONITORING CLINIC", Heading = "Silent Killer: Control Blood Pressure",
            Subtitle = "High BP ke lakshan turant nahi dikhte. Daily walk, kam namak, aur timely doctor consultation se BP control karein."
        },
        new() {
            CategorySlug = "cardiology", Order = 3, ImageUrl = "images/template/template_03.svg",
            Title = "Comprehensive Cardiac Screening Camp", StyleTheme = "Modern 3D",
            PrimaryColor = "#06b6d4", SecondaryColor = "#071329",
            BadgeText = "SPECIAL HEALTH CAMP", Heading = "Complete Heart Health Checkup Camp",
            Subtitle = "ECG, Lipid Profile, 2D Echo aur Chief Cardiologist consultation par vishesh medical suvidha uplabdh."
        },
        new() {
            CategorySlug = "cardiology", Order = 4, ImageUrl = "images/template/template_04.svg",
            Title = "Preventive Heart Care & Healthy Diet", StyleTheme = "Emerald Minimal",
            PrimaryColor = "#10b981", SecondaryColor = "#061a2b",
            BadgeText = "HEALTHY LIFESTYLE", Heading = "Eat Healthy, Live Strong Heart",
            Subtitle = "Trans-fat aur junk food se door rahein. Daily 30 minute cardio exercise aapke dil ko lambi umar deta hai."
        },
        new() {
            CategorySlug = "cardiology", Order = 5, ImageUrl = "images/template/template_05.svg",
            Title = "Advanced Interventional Cardiology OPD", StyleTheme = "Futuristic Glow",
            PrimaryColor = "#818cf8", SecondaryColor = "#0a1324",
            BadgeText = "SUPER SPECIALIST OPD", Heading = "Advanced Cardiac Interventions",
            Subtitle = "Angiography, Angioplasty aur Pacemaker implantation ke liye modern cath-lab aur experienced team."
        },

        // Category 2: Dental (6-10)
        new() {
            CategorySlug = "dental", Order = 6, ImageUrl = "images/template/template_06.svg",
            Title = "Smile Makeover & Cosmetic Dentistry", StyleTheme = "Ice Clean",
            PrimaryColor = "#22d3ee", SecondaryColor = "#061f2d",
            BadgeText = "COSMETIC DENTISTRY", Heading = "Design Your Perfect Confident Smile",
            Subtitle = "Veneers, Teeth Alignment aur Smile Designing se paayein naturally bright aur sparkling smile."
        },
        new() {
            CategorySlug = "dental", Order = 7, ImageUrl = "images/template/template_07.svg",
            Title = "दर्द-रहित रूट कैनाल एवं आधुनिक डेंटल इंप्लांट्स", StyleTheme = "Glassmorphism",
            PrimaryColor = "#2dd4bf", SecondaryColor = "#07242e",
            BadgeText = "दंत चिकित्सा क्लीनिक", Heading = "दांतों के दर्द से पाएं तुरंत और सुरक्षित राहत",
            Subtitle = "आधुनिक रोटरी टेक्नोलॉजी द्वारा सिंगल सिटिंग रूट कैनाल, लेजर टीथ व्हाइटनिंग व डेंटल इंप्लांट्स।",
            Language = "hi"
        },
        new() {
            CategorySlug = "dental", Order = 8, ImageUrl = "images/template/template_08.svg",
            Title = "Complete Dental Health Checkup", StyleTheme = "Minimal Clean",
            PrimaryColor = "#34d399", SecondaryColor = "#081e28",
            BadgeText = "ORAL HYGIENE CARE", Heading = "Routine Dental Examination & Cleaning",
            Subtitle = "Har 6 mahine me professional scaling aur cavity checkup daanton ko lambe samay tak surakshit rakhta hai."
        },
        new() {
            CategorySlug = "dental", Order = 9, ImageUrl = "images/template/template_09.svg",
            Title = "Pediatric Dentistry - Kids Smile Care", StyleTheme = "Warm Friendly",
            PrimaryColor = "#fbbf24", SecondaryColor = "#0d2836",
            BadgeText = "KIDS SMILE SPECIALIST", Heading = "Healthy Teeth, Happy Children",
            Subtitle = "Bachho ke daanto me cavity prevention, fluoride therapy, aur friendly atmosphere me painless treatment."
        },
        new() {
            CategorySlug = "dental", Order = 10, ImageUrl = "images/template/template_10.svg",
            Title = "Laser Teeth Whitening Clinic", StyleTheme = "Modern 3D",
            PrimaryColor = "#67e8f9", SecondaryColor = "#081c2e",
            BadgeText = "LASER DENTISTRY", Heading = "Instant Laser Teeth Whitening",
            Subtitle = "Sirf 45 minute ke single session me payein 3-4 shade brighter smile bina kisi tooth sensitivity ke."
        },

        // Category 3: Orthopedics (11-14)
        new() {
            CategorySlug = "orthopedics", Order = 11, ImageUrl = "images/template/template_11.svg",
            Title = "जोड़ों एवं रीढ़ की हड्डी के दर्द का संपूर्ण इलाज", StyleTheme = "Dark Metallic",
            PrimaryColor = "#38bdf8", SecondaryColor = "#0f172a",
            BadgeText = "ऑर्थोपेडिक एवं स्पाइन केयर", Heading = "जोड़ों के दर्द को कहें हमेशा के लिए अलविदा",
            Subtitle = "घुटनों व कमर दर्द, आर्थराइटिस एवं स्पोर्ट्स इंजरी का अनुभवी सर्जन द्वारा आधुनिक और सफल उपचार।",
            Language = "hi"
        },
        new() {
            CategorySlug = "orthopedics", Order = 12, ImageUrl = "images/template/template_12.svg",
            Title = "Spine Health & Slip Disc Recovery", StyleTheme = "Futuristic Glow",
            PrimaryColor = "#818cf8", SecondaryColor = "#0b1426",
            BadgeText = "SPINE CARE CENTER", Heading = "Healthy Spine, Active Life Without Pain",
            Subtitle = "Back pain, sciatica aur slip disc ke liye non-surgical rehabilitation aur minimally invasive guidance."
        },
        new() {
            CategorySlug = "orthopedics", Order = 13, ImageUrl = "images/template/template_13.svg",
            Title = "Robotic Knee & Hip Replacement", StyleTheme = "Modern 3D",
            PrimaryColor = "#60a5fa", SecondaryColor = "#0d1b2a",
            BadgeText = "ROBOTIC JOINT REPLACEMENT", Heading = "Walk Again With Confidence",
            Subtitle = "Sub-millimeter precision wali robotic joint surgery se fast recovery aur natural joint mobility payein."
        },
        new() {
            CategorySlug = "orthopedics", Order = 14, ImageUrl = "images/template/template_14.svg",
            Title = "Bone Mineral Density (BMD) Camp", StyleTheme = "Minimal Clean",
            PrimaryColor = "#2dd4bf", SecondaryColor = "#0a192f",
            BadgeText = "OSTEOPOROSIS SCREENING", Heading = "Check Your Bone Strength Today",
            Subtitle = "Kamzor haddiyo ko majboot karein. Calcium, Vitamin D deficiency aur osteoporosis ka samay par pata lagayein."
        },

        // Category 4: Pediatrics (15-18)
        new() {
            CategorySlug = "pediatrics", Order = 15, ImageUrl = "images/template/template_15.svg",
            Title = "Child Vaccination & Immunization Alert", StyleTheme = "Warm Friendly",
            PrimaryColor = "#f59e0b", SecondaryColor = "#0a2540",
            BadgeText = "IMMUNIZATION SCHEDULE", Heading = "Never Miss a Vaccine For Your Child",
            Subtitle = "Birth se lekar 16 saal tak ke samast anivarya teekakaran (WHO certified) clinic par uplabdh hain."
        },
        new() {
            CategorySlug = "pediatrics", Order = 16, ImageUrl = "images/template/template_16.svg",
            Title = "शिशु एवं बाल स्वास्थ्य - संपूर्ण टीकाकरण", StyleTheme = "Pastel Care",
            PrimaryColor = "#f472b6", SecondaryColor = "#10233b",
            BadgeText = "बाल रोग विशेषज्ञ ओपीडी", Heading = "बच्चों की बेहतर सेहत और सुरक्षित भविष्य",
            Subtitle = "जन्म से लेकर 18 वर्ष तक के बच्चों का संपूर्ण टीकाकरण, पोषण परामर्श एवं मौसमी बीमारियों का इलाज।",
            Language = "hi"
        },
        new() {
            CategorySlug = "pediatrics", Order = 17, ImageUrl = "images/template/template_17.svg",
            Title = "Child Nutrition & Growth Milestones", StyleTheme = "Emerald Minimal",
            PrimaryColor = "#10b981", SecondaryColor = "#0a2630",
            BadgeText = "CHILD WELLNESS", Heading = "Track Your Child's Growth & Nutrition",
            Subtitle = "Height, weight, immunity aur brain development ke liye customized dietary charts aur pediatrician guidance."
        },
        new() {
            CategorySlug = "pediatrics", Order = 18, ImageUrl = "images/template/template_18.svg",
            Title = "Pediatric Emergency & Fever OPD", StyleTheme = "Alert Glow",
            PrimaryColor = "#fb7185", SecondaryColor = "#182038",
            BadgeText = "24x7 CHILD CARE", Heading = "Emergency Child Care & Fever Clinic",
            Subtitle = "Achanak tej bukhar, dehydration ya saans lene me takleef par turant pediatric emergency suvidha."
        },

        // Category 5: Gynecology (19-22)
        new() {
            CategorySlug = "gynecology", Order = 19, ImageUrl = "images/template/template_19.svg",
            Title = "Safe Motherhood & Antenatal Care", StyleTheme = "Rose Glass",
            PrimaryColor = "#f472b6", SecondaryColor = "#1f102a",
            BadgeText = "MATERNITY CLINIC", Heading = "Compassionate Care for Mother & Baby",
            Subtitle = "Pregnancy ke har mahine expert guidance, high-resolution ultrasound, aur safe institutional delivery."
        },
        new() {
            CategorySlug = "gynecology", Order = 20, ImageUrl = "images/template/template_20.svg",
            Title = "Women Wellness & Preventive Screenings", StyleTheme = "Violet Elegance",
            PrimaryColor = "#c084fc", SecondaryColor = "#221330",
            BadgeText = "WOMEN HEALTH MATTERS", Heading = "Prioritize Your Health, Dear Women",
            Subtitle = "Pap smear, breast screening, anemia check aur bone health consultation par complete female wellness package."
        },
        new() {
            CategorySlug = "gynecology", Order = 21, ImageUrl = "images/template/template_21.svg",
            Title = "मातृत्व एवं महिला स्वास्थ्य परामर्श", StyleTheme = "Modern 3D",
            PrimaryColor = "#e879f9", SecondaryColor = "#281230",
            BadgeText = "स्त्री एवं प्रसूति रोग ओपीडी", Heading = "सुरक्षित मातृत्व, स्वस्थ नवजात शिशु",
            Subtitle = "गर्भावस्था की नियमित जांच, हाई-रिस्क प्रेगनेंसी केयर एवं अनुभवी महिला रोग विशेषज्ञ द्वारा परामर्श।",
            Language = "hi"
        },
        new() {
            CategorySlug = "gynecology", Order = 22, ImageUrl = "images/template/template_22.svg",
            Title = "High-Risk Pregnancy & Normal Delivery Support", StyleTheme = "Warm Coral",
            PrimaryColor = "#fb923c", SecondaryColor = "#1a1226",
            BadgeText = "OBSTETRICS EXPERT", Heading = "Expert Care in High-Risk Pregnancy",
            Subtitle = "Gestational diabetes, hypertension ya twin pregnancy me specialized monitoring aur painless normal delivery."
        },

        // Category 6: General Health (23-28)
        new() {
            CategorySlug = "general-health", Order = 23, ImageUrl = "images/template/template_23.svg",
            Title = "Executive Full Body Health Checkup", StyleTheme = "Emerald Medical",
            PrimaryColor = "#10b981", SecondaryColor = "#062226",
            BadgeText = "PREVENTIVE HEALTHCARE", Heading = "Annual Full Body Diagnostic Checkup",
            Subtitle = "60+ parameters: Liver, Kidney, Lipid, Sugar, CBC, Thyroid aur Physician Review ek hi chhat ke neeche."
        },
        new() {
            CategorySlug = "general-health", Order = 24, ImageUrl = "images/template/template_24.svg",
            Title = "World Diabetes Day - Manage Sugar Levels", StyleTheme = "Blue Circle",
            PrimaryColor = "#38bdf8", SecondaryColor = "#071c36",
            BadgeText = "DIABETES AWARENESS", Heading = "Beat Diabetes With Smart Monitoring",
            Subtitle = "HbA1c test, diabetic foot check, eye screening aur customized insulin/diet management plan."
        },
        new() {
            CategorySlug = "general-health", Order = 25, ImageUrl = "images/template/template_25.svg",
            Title = "Seasonal Illness & Dengue Prevention", StyleTheme = "Aqua Clean",
            PrimaryColor = "#06b6d4", SecondaryColor = "#091c29",
            BadgeText = "SEASONAL ALERT", Heading = "Protect Your Family From Dengue & Malaria",
            Subtitle = "Platelet monitoring, rapid antigen testing aur expert physician care 24 ghante uplabdh."
        },
        new() {
            CategorySlug = "general-health", Order = 26, ImageUrl = "images/template/template_26.svg",
            Title = "डायबिटीज एवं ब्लड प्रेशर नियंत्रण शिविर", StyleTheme = "Teal Minimal",
            PrimaryColor = "#2dd4bf", SecondaryColor = "#092233",
            BadgeText = "विशेष स्वास्थ्य शिविर", Heading = "डायबिटीज और हाई बीपी पर रखें पूरा नियंत्रण",
            Subtitle = "संतुलित आहार, नियमित जांच और सही दवाओं से अपनी शुगर को कंट्रोल में रखें और स्वस्थ रहें।",
            Language = "hi"
        },
        new() {
            CategorySlug = "general-health", Order = 27, ImageUrl = "images/template/template_27.svg",
            Title = "Senior Citizen Geriatric Care Package", StyleTheme = "Prestige Gold",
            PrimaryColor = "#f59e0b", SecondaryColor = "#0d1b2a",
            BadgeText = "ELDERLY CARE", Heading = "Specialized Healthcare for Senior Citizens",
            Subtitle = "Home blood sample collection, priority OPD appointments, aur regular chronic disease management."
        },
        new() {
            CategorySlug = "general-health", Order = 28, ImageUrl = "images/template/template_28.svg",
            Title = "Boost Your Immunity This Season", StyleTheme = "Vibrant Green",
            PrimaryColor = "#34d399", SecondaryColor = "#082622",
            BadgeText = "IMMUNITY BOOSTER", Heading = "Strengthen Your Natural Defense",
            Subtitle = "Seasonal infections se bachein. Nutrition guidance, essential vitamins aur preventive health tips."
        },

        // Category 7: Eye, Skin & ENT (29-34)
        new() {
            CategorySlug = "eye-skin-ent", Order = 29, ImageUrl = "images/template/template_29.svg",
            Title = "Glaucoma & Cataract Screening Camp", StyleTheme = "Modern 3D",
            PrimaryColor = "#38bdf8", SecondaryColor = "#08162b",
            BadgeText = "EYE CARE CLINIC", Heading = "Clear Vision For a Brighter Future",
            Subtitle = "Advanced non-contact tonometry, retina evaluation aur micro-incision cataract surgery consultation."
        },
        new() {
            CategorySlug = "eye-skin-ent", Order = 30, ImageUrl = "images/template/template_30.svg",
            Title = "Digital Eye Strain & Computer Vision", StyleTheme = "Cyan Glow",
            PrimaryColor = "#00f2fe", SecondaryColor = "#091b33",
            BadgeText = "COMPUTER VISION CARE", Heading = "Relief From Digital Screen Eye Strain",
            Subtitle = "Dry eyes, sir dard aur blurry vision se chhutkaara payein. 20-20-20 rule apnayein aur eye test karwayein."
        },
        new() {
            CategorySlug = "eye-skin-ent", Order = 31, ImageUrl = "images/template/template_31.svg",
            Title = "त्वचा एवं सौंदर्य चिकित्सा परामर्श", StyleTheme = "Rose Gold",
            PrimaryColor = "#f472b6", SecondaryColor = "#241624",
            BadgeText = "डर्मेटोलॉजी क्लीनिक", Heading = "स्वस्थ, चमकदार एवं सुंदर त्वचा पाएं",
            Subtitle = "मुंहासे, बालों का झड़ना, डार्क स्पॉट्स एवं आधुनिक लेजर व पीआरपी थेरेपी द्वारा उपचार।",
            Language = "hi"
        },
        new() {
            CategorySlug = "eye-skin-ent", Order = 32, ImageUrl = "images/template/template_32.svg",
            Title = "Acne & Hair Fall Solution OPD", StyleTheme = "Fresh Aqua",
            PrimaryColor = "#2dd4bf", SecondaryColor = "#15222b",
            BadgeText = "TRICHOLOGY CLINIC", Heading = "Advanced Acne & Hair Loss Solutions",
            Subtitle = "PRP therapy, scalp health analysis aur customized scientific treatment se paayein naturally dense hair."
        },
        new() {
            CategorySlug = "eye-skin-ent", Order = 33, ImageUrl = "images/template/template_33.svg",
            Title = "Comprehensive ENT & Sinusitis Care", StyleTheme = "Deep Ocean",
            PrimaryColor = "#60a5fa", SecondaryColor = "#091a2e",
            BadgeText = "ENT SPECIALIST", Heading = "Breathe Easy, Hear Clearly",
            Subtitle = "Chronic sinusitis, allergy, tonsil infection aur vertigo ka endoscopic examination dwara satik ilaaj."
        },
        new() {
            CategorySlug = "eye-skin-ent", Order = 34, ImageUrl = "images/template/template_34.svg",
            Title = "Audiometry & Hearing Assessment Camp", StyleTheme = "Sound Wave",
            PrimaryColor = "#38bdf8", SecondaryColor = "#081d30",
            BadgeText = "HEARING HEALTH", Heading = "Free Audiometry Hearing Test Camp",
            Subtitle = "Digital pure-tone audiometry aur latest discreet invisible digital hearing aid trials uplabdh."
        },

        // Category 8: Emergency (35-38)
        new() {
            CategorySlug = "emergency", Order = 35, ImageUrl = "images/template/template_35.svg",
            Title = "24 घंटे आपातकालीन एवं ट्रामा केयर सेवा", StyleTheme = "Alert Red",
            PrimaryColor = "#ef4444", SecondaryColor = "#2a0c12",
            BadgeText = "24x7 इमरजेंसी एवं आईसीयू", Heading = "आपातकालीन स्थिति में तुरंत चिकित्सा सहायता",
            Subtitle = "24 घंटे एम्बुलेंस, वेंटिलेटर, क्रिटिकल केयर आईसीयू एवं विशेषज्ञ डॉक्टरों की टीम सदैव तैयार।",
            Language = "hi"
        },
        new() {
            CategorySlug = "emergency", Order = 36, ImageUrl = "images/template/template_36.svg",
            Title = "24/7 Advanced Cardiac Ambulance", StyleTheme = "Emergency Yellow",
            PrimaryColor = "#f59e0b", SecondaryColor = "#260e0e",
            BadgeText = "EMERGENCY HELPLINE", Heading = "Rapid Response Cardiac Ambulance 24x7",
            Subtitle = "Ventilator, defibrillator, oxygen support aur paramedic team se lais ambulance sirf ek call par."
        },
        new() {
            CategorySlug = "emergency", Order = 37, ImageUrl = "images/template/template_37.svg",
            Title = "Brain Stroke Golden Hour Protocol", StyleTheme = "Crimson Precision",
            PrimaryColor = "#f43f5e", SecondaryColor = "#240b15",
            BadgeText = "STROKE READY HOSPITAL", Heading = "Act FAST: Time is Brain in Stroke",
            Subtitle = "Face drooping, Arm weakness, Speech difficulty. Pehle 4.5 ghante me thrombolysis se jaan bach sakti hai."
        },
        new() {
            CategorySlug = "emergency", Order = 38, ImageUrl = "images/template/template_38.svg",
            Title = "Advanced Multi-Specialty ICU & Critical Care", StyleTheme = "Dark Monitor",
            PrimaryColor = "#06b6d4", SecondaryColor = "#071b26",
            BadgeText = "CRITICAL CARE ICU", Heading = "Round-The-Clock Intensive Care Unit",
            Subtitle = "Intensivists aur dedicated critical care nursing team dwara 1:1 patient care aur life support monitoring."
        },

        // Category 9: Festivals (39-46)
        new() {
            CategorySlug = "festivals", Order = 39, ImageUrl = "images/template/template_39.svg",
            Title = "शुभ दीपावली: उत्तम स्वास्थ्य और खुशहाली की शुभकामनाएं", StyleTheme = "Festive Gold",
            PrimaryColor = "#fbbf24", SecondaryColor = "#1e1306",
            BadgeText = "हार्दिक शुभकामनाएं", Heading = "शुभ दीपावली: स्वस्थ रहें, खुशहाल रहें",
            Subtitle = "दीपों का यह पावन पर्व आपके और आपके परिवार के जीवन में उत्तम स्वास्थ्य, सुख और समृद्धि लाए।",
            Language = "hi"
        },
        new() {
            CategorySlug = "festivals", Order = 40, ImageUrl = "images/template/template_40.svg",
            Title = "होली की हार्दिक शुभकामनाएं - सुरक्षित एवं हर्बल होली", StyleTheme = "Vibrant Festive",
            PrimaryColor = "#f43f5e", SecondaryColor = "#1e1026",
            BadgeText = "होली शुभकामनाएं", Heading = "रंगों का त्योहार, खुशियों और सेहत के साथ",
            Subtitle = "प्राकृतिक एवं हर्बल रंगों का प्रयोग करें। आंखों और त्वचा की सुरक्षा रखें। सपरिवार शुभ होली!",
            Language = "hi"
        },
        new() {
            CategorySlug = "festivals", Order = 41, ImageUrl = "images/template/template_41.svg",
            Title = "ईद मुबारक - सेहत और अमन की दुआएं", StyleTheme = "Emerald Royal",
            PrimaryColor = "#34d399", SecondaryColor = "#052219",
            BadgeText = "ईद मुबारक शुभकामनाएं", Heading = "ईद मुबारक: खुशियों और सेहत से भरा त्योहार",
            Subtitle = "आप सभी के जीवन में बरकत, खुशहाली और उत्तम स्वास्थ्य बना रहे। परिवार सहित मुबारकबाद।",
            Language = "hi"
        },
        new() {
            CategorySlug = "festivals", Order = 42, ImageUrl = "images/template/template_42.svg",
            Title = "Happy New Year - Invest in Your Health", StyleTheme = "Champagne Gold",
            PrimaryColor = "#fbbf24", SecondaryColor = "#0c1730",
            BadgeText = "NEW YEAR RESOLUTION", Heading = "Step Into the New Year With Good Health",
            Subtitle = "Is naye saal par daily fitness, healthy khana aur routine checkups ka sankalp lein."
        },
        new() {
            CategorySlug = "festivals", Order = 43, ImageUrl = "images/template/template_43.svg",
            Title = "National Doctor's Day Celebration", StyleTheme = "Prestige Medical",
            PrimaryColor = "#38bdf8", SecondaryColor = "#09172e",
            BadgeText = "DOCTOR'S DAY", Heading = "Honoring Our Healthcare Heroes",
            Subtitle = "Niswarth sewa, samparpan aur jeevan bachane wale sabhi doctors aur medical staff ko koti-koti pranam."
        },
        new() {
            CategorySlug = "festivals", Order = 44, ImageUrl = "images/template/template_44.svg",
            Title = "International Day of Yoga Celebration", StyleTheme = "Peaceful Teal",
            PrimaryColor = "#2dd4bf", SecondaryColor = "#092429",
            BadgeText = "YOGA FOR HEALTH", Heading = "Yoga: Harmony of Body, Mind & Soul",
            Subtitle = "Daily pranayama aur asanas se mental stress door karein aur immunity badhayein."
        },
        new() {
            CategorySlug = "festivals", Order = 45, ImageUrl = "images/template/template_45.svg",
            Title = "World Health Day - Health For All", StyleTheme = "Global Emerald",
            PrimaryColor = "#10b981", SecondaryColor = "#07202b",
            BadgeText = "WORLD HEALTH DAY", Heading = "Universal Healthcare: Health For Everyone",
            Subtitle = "Swasthya har nagrik ka maulik adhikaar hai. Clean water, nutritious food aur timely medical care sabke liye."
        },
        new() {
            CategorySlug = "festivals", Order = 46, ImageUrl = "images/template/template_46.svg",
            Title = "Independence Day - Freedom From Diseases", StyleTheme = "Tiranga Prestige",
            PrimaryColor = "#f59e0b", SecondaryColor = "#0a192f",
            BadgeText = "78TH INDEPENDENCE DAY", Heading = "Swasth Bharat, Samriddh Bharat",
            Subtitle = "Rashtra nirman me swasth sharir hi sabse badi taqat hai. Swasthya ki aazadi, khushiyon ki aazadi."
        },

        // Category 10: Announcements (47-50)
        new() {
            CategorySlug = "announcements", Order = 47, ImageUrl = "images/template/template_47.svg",
            Title = "Grand Inauguration of New Specialty Clinic", StyleTheme = "Luxury Black & Gold",
            PrimaryColor = "#fbbf24", SecondaryColor = "#12141c",
            BadgeText = "GRAND OPENING", Heading = "Grand Inauguration of New Medical Center",
            Subtitle = "Shehar me vishwasneey aur aadhunik chikitsa suvidhaon ke sath naye clinic ka shubharambh. Aapka swagat hai!"
        },
        new() {
            CategorySlug = "announcements", Order = 48, ImageUrl = "images/template/template_48.svg",
            Title = "Visiting Super Specialist Announcement", StyleTheme = "Executive Blue",
            PrimaryColor = "#00f2fe", SecondaryColor = "#091629",
            BadgeText = "VISITING EXPERT", Heading = "Renowned Super Specialist Now in Town",
            Subtitle = "Metropolitan cities ke leading specialists ab aapke shahar me har mahine niyamit paramarsh ke liye uplabdh."
        },
        new() {
            CategorySlug = "announcements", Order = 49, ImageUrl = "images/template/template_49.svg",
            Title = "निःशुल्क संपूर्ण स्वास्थ्य जांच एवं परामर्श शिविर", StyleTheme = "Sky Modern",
            PrimaryColor = "#38bdf8", SecondaryColor = "#071c2b",
            BadgeText = "फ्री हेल्थ कैंप", Heading = "निःशुल्क स्वास्थ्य जांच एवं परामर्श शिविर",
            Subtitle = "आगामी रविवार: ब्लड प्रेशर, शुगर, ईसीजी एवं विशेषज्ञ डॉक्टरों द्वारा फ्री कंसल्टेशन सुविधा।",
            Language = "hi"
        },
        new() {
            CategorySlug = "announcements", Order = 50, ImageUrl = "images/template/template_50.svg",
            Title = "Online Video Consultation Now Available", StyleTheme = "Telehealth Digital",
            PrimaryColor = "#818cf8", SecondaryColor = "#131633",
            BadgeText = "TELEHEALTH SERVICE", Heading = "Consult Doctor Online From Home",
            Subtitle = "Ab ghar baithe smartphone dwara HD video consultation aur digital prescription suvidha uplabdh."
        }
    };

    public static string GenerateConfigJson(TemplateSeedItem item, string categoryName)
    {
        bool isHindi = item.Language == "hi" || 
            System.Text.RegularExpressions.Regex.IsMatch(item.Heading + " " + item.Subtitle + " " + item.Title, @"[\u0900-\u097F]");

        var state = new BannerCustomizationState
        {
            TemplateType = categoryName,
            Language = isHindi ? "hi" : "en",
            Heading = item.Heading,
            Subtitle = item.Subtitle,
            BadgeText = item.BadgeText,
            AspectRatio = item.AspectRatio,
            PrimaryColor = item.PrimaryColor,
            SecondaryColor = item.SecondaryColor,
            TextColor = "#ffffff",
            BackgroundImageUrl = item.ImageUrl,
            BackgroundStyle = "ai-image",
            FontFamily = isHindi ? "Noto Sans Devanagari" : "Poppins",
            HeadingFontFamily = isHindi ? "Noto Sans Devanagari" : "Poppins",
            SubtitleFontFamily = isHindi ? "Noto Sans Devanagari" : "Poppins",
            DoctorCardFontFamily = isHindi ? "Noto Sans Devanagari" : "Poppins",
            DoctorName = isHindi ? "डॉ. अभिषेक वर्मा, MBBS, MD" : "Dr. Abhishek Verma, MBBS, MD",
            Specialization = isHindi ? categoryName + " विशेषज्ञ" : categoryName + " Consultant",
            HospitalName = isHindi ? "क्लिक योर हॉस्पिटल सुपर स्पेशलिटी क्लीनिक" : "CYH Super Specialty Clinic",
            ContactPhone = "+91 98765 43210",
            EmergencyPhone = "Helpline: 1800-123-4567",
            Address = "Main Boulevard, Healthcare City",
            Website = "https://www.clickyourhospital.com",
            LogoUrl = "images/branding/cyh-logo.svg",
            LogoPosition = "bottom-center",
            LogoSizePercent = 14,
            LogoPaddingBottom = 30,
            ShowLogoBadge = true,
            LogoBadgeText = isHindi ? "CYH डॉक्टरहब प्रमाणित स्वास्थ्य ब्रांडिंग" : "CYH DOCTORHUB OFFICIAL BRANDING"
        };

        return JsonSerializer.Serialize(state);
    }
}

-- ============================================================
--  BAZA PODATAKA: OSIGURAVAJUCA KOMPANIJA
--  Uskladjeno sa NHibernate/FluentNHibernate projektom v3
--  
--  RAZLIKE u odnosu na prvu skriptu:
--  1. GENERATED ALWAYS AS IDENTITY zamenjeno sa SEQUENCE-ovima
--     (NHibernate zahteva eksplicitne sequence-ove)
--  2. Kolona limit_pokric_a preimenovana u LIMIT_POKRICA
--     (izbjegavanje specijalnih znakova u imenima kolona)
--  3. Dodata tabela OBLAST_PROCENE (nedostajala je OBLAST_ID_SEQ)
--  4. Dodate sve SEQUENCE-ove koje mapiranja koriste
-- ============================================================

-- ============================================================
-- SEKCIJA 1: BRISANJE POSTOJECIH OBJEKATA
-- ============================================================

BEGIN
  FOR t IN (SELECT table_name FROM user_tables) LOOP
    EXECUTE IMMEDIATE 'DROP TABLE "' || t.table_name || '" CASCADE CONSTRAINTS';
  END LOOP;
END;
/

BEGIN
  FOR s IN (SELECT sequence_name FROM user_sequences) LOOP
    EXECUTE IMMEDIATE 'DROP SEQUENCE "' || s.sequence_name || '"';
  END LOOP;
END;
/

-- ============================================================
-- SEKCIJA 2: SEQUENCE-OVI
-- (NHibernate GeneratedBy.Sequence() zahteva eksplicitne seq)
-- ============================================================

CREATE SEQUENCE KLIJENT_ID_SEQ         START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE KONTAKT_ID_SEQ         START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE OSOBLJE_ID_SEQ         START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE OBLAST_ID_SEQ          START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE POLISA_ID_SEQ          START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE ULOGA_ID_SEQ           START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE KORISNIK_ID_SEQ        START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE POKRICE_ID_SEQ         START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE ISTORIJA_ID_SEQ        START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE VOZILO_ID_SEQ          START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE NEKRETNINA_ID_SEQ      START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE STETA_ID_SEQ           START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE FAZA_ID_SEQ            START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE PROCENA_ID_SEQ         START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE OSTECENIPREDMET_ID_SEQ START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;
CREATE SEQUENCE OSTECENOLICE_ID_SEQ    START WITH 1   INCREMENT BY 1 NOCACHE NOCYCLE;

-- ============================================================
-- SEKCIJA 3: KREIRANJE TABELA
-- ============================================================

-- ------------------------------------------------------------
-- 3.1 KLIJENTI
-- ------------------------------------------------------------

CREATE TABLE KLIJENT (
    klijent_id        NUMBER          DEFAULT KLIJENT_ID_SEQ.NEXTVAL PRIMARY KEY,
    naziv             VARCHAR2(200)   NOT NULL,
    adresa            VARCHAR2(300),
    telefon           VARCHAR2(30),
    email             VARCHAR2(100),
    status            VARCHAR2(20)    DEFAULT 'AKTIVAN'
                      CONSTRAINT chk_kl_status CHECK (status IN ('AKTIVAN','NEAKTIVAN','BLOKIRAN')),
    datum_registracije DATE           DEFAULT SYSDATE NOT NULL,
    tip_klijenta      VARCHAR2(20)    NOT NULL
                      CONSTRAINT chk_kl_tip CHECK (tip_klijenta IN ('FIZICKO_LICE','PRAVNO_LICE','JAVNA_INSTITUCIJA'))
);

CREATE TABLE FIZICKO_LICE (
    klijent_id        NUMBER          PRIMARY KEY,
    jmbg              CHAR(13)        UNIQUE NOT NULL,
    datum_rodjenja    DATE,
    zanimanje         VARCHAR2(100),
    CONSTRAINT fk_fl_klijent FOREIGN KEY (klijent_id)
        REFERENCES KLIJENT(klijent_id) ON DELETE CASCADE
);

CREATE TABLE PRAVNO_LICE (
    klijent_id        NUMBER          PRIMARY KEY,
    pib               VARCHAR2(9)     UNIQUE NOT NULL,
    maticni_broj      VARCHAR2(8)     UNIQUE NOT NULL,
    delatnost         VARCHAR2(200),
    CONSTRAINT fk_pl_klijent FOREIGN KEY (klijent_id)
        REFERENCES KLIJENT(klijent_id) ON DELETE CASCADE
);

CREATE TABLE JAVNA_INSTITUCIJA (
    klijent_id        NUMBER          PRIMARY KEY,
    pib               VARCHAR2(9)     UNIQUE NOT NULL,
    maticni_broj      VARCHAR2(8)     UNIQUE NOT NULL,
    delatnost         VARCHAR2(200),
    nivo_institucije  VARCHAR2(20)    NOT NULL
                      CONSTRAINT chk_ji_nivo CHECK (nivo_institucije IN ('REPUBLICKA','POKRAJINSKA','GRADSKA','OPSTINSKA')),
    CONSTRAINT fk_ji_klijent FOREIGN KEY (klijent_id)
        REFERENCES KLIJENT(klijent_id) ON DELETE CASCADE
);

CREATE TABLE KONTAKT_OSOBA (
    kontakt_id        NUMBER          DEFAULT KONTAKT_ID_SEQ.NEXTVAL PRIMARY KEY,
    klijent_id        NUMBER          NOT NULL,
    ime               VARCHAR2(80)    NOT NULL,
    prezime           VARCHAR2(80)    NOT NULL,
    telefon           VARCHAR2(30),
    email             VARCHAR2(100),
    funkcija          VARCHAR2(100),
    CONSTRAINT fk_ko_klijent FOREIGN KEY (klijent_id)
        REFERENCES KLIJENT(klijent_id) ON DELETE CASCADE
);

-- ------------------------------------------------------------
-- 3.2 OSOBLJE
-- ------------------------------------------------------------

CREATE TABLE OSOBLJE (
    osoblje_id        NUMBER          DEFAULT OSOBLJE_ID_SEQ.NEXTVAL PRIMARY KEY,
    ime               VARCHAR2(80)    NOT NULL,
    prezime           VARCHAR2(80)    NOT NULL,
    jmbg              CHAR(13)        UNIQUE NOT NULL,
    adresa            VARCHAR2(300),
    telefon           VARCHAR2(30),
    email             VARCHAR2(100),
    datum_angazovanja DATE            DEFAULT SYSDATE NOT NULL,
    status            VARCHAR2(20)    DEFAULT 'AKTIVAN'
                      CONSTRAINT chk_os_status CHECK (status IN ('AKTIVAN','NEAKTIVAN','SUSPENDOVAN')),
    tip_osoblja       VARCHAR2(20)    NOT NULL
                      CONSTRAINT chk_os_tip CHECK (tip_osoblja IN ('AGENT','PROCENITELJ','LEKAR','PRAVNIK','OSTALO'))
);

CREATE TABLE AGENT (
    osoblje_id        NUMBER          PRIMARY KEY,
    tip_agenta        VARCHAR2(10)    NOT NULL
                      CONSTRAINT chk_ag_tip CHECK (tip_agenta IN ('INTERNI','EKSTERNI')),
    licenca           VARCHAR2(50)    UNIQUE NOT NULL,
    region_rada       VARCHAR2(100),
    provizija_procenat NUMBER(5,2)    DEFAULT 5.00
                      CONSTRAINT chk_ag_prov CHECK (provizija_procenat BETWEEN 0 AND 100),
    CONSTRAINT fk_ag_osoblje FOREIGN KEY (osoblje_id)
        REFERENCES OSOBLJE(osoblje_id) ON DELETE CASCADE
);

CREATE TABLE PROCENITELJ (
    osoblje_id        NUMBER          PRIMARY KEY,
    CONSTRAINT fk_pr_osoblje FOREIGN KEY (osoblje_id)
        REFERENCES OSOBLJE(osoblje_id) ON DELETE CASCADE
);

CREATE TABLE OBLAST_PROCENE (
    oblast_id         NUMBER          DEFAULT OBLAST_ID_SEQ.NEXTVAL PRIMARY KEY,
    osoblje_id        NUMBER          NOT NULL,
    oblast            VARCHAR2(20)    NOT NULL
                      CONSTRAINT chk_ob_oblast CHECK (oblast IN ('VOZILO','IMOVINA','ZDRAVSTVO','SPECIJALNE_STETE')),
    CONSTRAINT fk_ob_procenitelj FOREIGN KEY (osoblje_id)
        REFERENCES PROCENITELJ(osoblje_id) ON DELETE CASCADE,
    CONSTRAINT uq_ob_osoblje_oblast UNIQUE (osoblje_id, oblast)
);

CREATE TABLE LEKAR (
    osoblje_id        NUMBER          PRIMARY KEY,
    specijalizacija   VARCHAR2(100),
    licenca_broj      VARCHAR2(50),
    CONSTRAINT fk_lek_osoblje FOREIGN KEY (osoblje_id)
        REFERENCES OSOBLJE(osoblje_id) ON DELETE CASCADE
);

CREATE TABLE PRAVNIK (
    osoblje_id        NUMBER          PRIMARY KEY,
    tip_pravnika      VARCHAR2(10)    NOT NULL
                      CONSTRAINT chk_prav_tip CHECK (tip_pravnika IN ('INTERNI','EKSTERNI')),
    bar_broj          VARCHAR2(30),
    CONSTRAINT fk_prav_osoblje FOREIGN KEY (osoblje_id)
        REFERENCES OSOBLJE(osoblje_id) ON DELETE CASCADE
);

-- ------------------------------------------------------------
-- 3.3 POLISE
-- ------------------------------------------------------------

CREATE TABLE POLISA (
    polisa_id         NUMBER          DEFAULT POLISA_ID_SEQ.NEXTVAL PRIMARY KEY,
    broj_polise       VARCHAR2(30)    UNIQUE NOT NULL,
    tip_osiguranja    VARCHAR2(30)    NOT NULL
                      CONSTRAINT chk_pol_tip CHECK (tip_osiguranja IN (
                          'ZIVOTNO','ZDRAVSTVENO','IMOVINSKO','AUTO',
                          'PUTNO','POLJOPRIVREDNO','ODGOVORNOST','SPECIJALIZOVANO')),
    datum_zakljucenja DATE            DEFAULT SYSDATE NOT NULL,
    datum_pocetka     DATE            NOT NULL,
    datum_isteka      DATE            NOT NULL,
    status            VARCHAR2(20)    DEFAULT 'AKTIVNA'
                      CONSTRAINT chk_pol_status CHECK (status IN ('AKTIVNA','ISTEKLA','RASKINUTA','MIROVANJE','OBNOVLJENA')),
    osnovna_premija   NUMBER(15,2)    NOT NULL
                      CONSTRAINT chk_pol_prem CHECK (osnovna_premija > 0),
    valuta            VARCHAR2(5)     DEFAULT 'RSD'
                      CONSTRAINT chk_pol_valuta CHECK (valuta IN ('RSD','EUR','USD')),
    nacin_placanja    VARCHAR2(20)    DEFAULT 'MESECNO'
                      CONSTRAINT chk_pol_nacin CHECK (nacin_placanja IN ('JEDNOKRATNO','MESECNO','KVARTAL','POLUGODISNJE','GODISNJE')),
    agent_id          NUMBER,
    ugovarac_id       NUMBER          NOT NULL,
    CONSTRAINT fk_pol_agent FOREIGN KEY (agent_id)
        REFERENCES AGENT(osoblje_id),
    CONSTRAINT fk_pol_ugovarac FOREIGN KEY (ugovarac_id)
        REFERENCES KLIJENT(klijent_id),
    CONSTRAINT chk_pol_datumi CHECK (datum_isteka > datum_pocetka)
);

CREATE TABLE ULOGA_KLIJENTA (
    uloga_id          NUMBER          DEFAULT ULOGA_ID_SEQ.NEXTVAL PRIMARY KEY,
    klijent_id        NUMBER          NOT NULL,
    polisa_id         NUMBER          NOT NULL,
    tip_uloge         VARCHAR2(20)    NOT NULL
                      CONSTRAINT chk_ul_tip CHECK (tip_uloge IN ('UGOVARAC','OSIGURANIK','KORISNIK','OSTECENO_LICE')),
    CONSTRAINT fk_ul_klijent FOREIGN KEY (klijent_id)
        REFERENCES KLIJENT(klijent_id) ON DELETE CASCADE,
    CONSTRAINT fk_ul_polisa FOREIGN KEY (polisa_id)
        REFERENCES POLISA(polisa_id) ON DELETE CASCADE,
    CONSTRAINT uq_ul_kl_pol_tip UNIQUE (klijent_id, polisa_id, tip_uloge)
);

CREATE TABLE ZIVOTNO_OSIGURANJE (
    polisa_id         NUMBER          PRIMARY KEY,
    suma_osiguranja   NUMBER(15,2)    NOT NULL
                      CONSTRAINT chk_zo_suma CHECK (suma_osiguranja > 0),
    tip_isplate       VARCHAR2(20)    DEFAULT 'JEDNOKRATNA'
                      CONSTRAINT chk_zo_tip CHECK (tip_isplate IN ('JEDNOKRATNA','MESECNA_RENTA','KOMBINOVANA')),
    CONSTRAINT fk_zo_polisa FOREIGN KEY (polisa_id)
        REFERENCES POLISA(polisa_id) ON DELETE CASCADE
);

CREATE TABLE KORISNIK_ISPLATE (
    korisnik_id       NUMBER          DEFAULT KORISNIK_ID_SEQ.NEXTVAL PRIMARY KEY,
    polisa_id         NUMBER          NOT NULL,
    klijent_id        NUMBER          NOT NULL,
    procenat_udela    NUMBER(5,2)     DEFAULT 100.00
                      CONSTRAINT chk_ki_proc CHECK (procenat_udela BETWEEN 0.01 AND 100),
    CONSTRAINT fk_ki_polisa FOREIGN KEY (polisa_id)
        REFERENCES ZIVOTNO_OSIGURANJE(polisa_id) ON DELETE CASCADE,
    CONSTRAINT fk_ki_klijent FOREIGN KEY (klijent_id)
        REFERENCES KLIJENT(klijent_id),
    CONSTRAINT uq_ki_polisa_klijent UNIQUE (polisa_id, klijent_id)
);

CREATE TABLE ZDRAVSTVENO_OSIGURANJE (
    polisa_id           NUMBER          PRIMARY KEY,
    mreza_ustanova      VARCHAR2(300),
    limit_specijalista  NUMBER(10,2),
    limit_stomatologa   NUMBER(10,2),
    limit_bolnickih     NUMBER(10,2),
    limit_bolnicki_dan  NUMBER(10,2),
    CONSTRAINT fk_zdro_polisa FOREIGN KEY (polisa_id)
        REFERENCES POLISA(polisa_id) ON DELETE CASCADE
);

CREATE TABLE VOZILO (
    vozilo_id         NUMBER          DEFAULT VOZILO_ID_SEQ.NEXTVAL PRIMARY KEY,
    registracija      VARCHAR2(20)    UNIQUE NOT NULL,
    marka             VARCHAR2(50)    NOT NULL,
    model             VARCHAR2(80)    NOT NULL,
    godina_proizvodnje NUMBER(4)      NOT NULL
                      CONSTRAINT chk_voz_god CHECK (godina_proizvodnje BETWEEN 1886 AND 2100),
    vlasnik_id        NUMBER          NOT NULL,
    CONSTRAINT fk_voz_vlasnik FOREIGN KEY (vlasnik_id)
        REFERENCES KLIJENT(klijent_id)
);

CREATE TABLE AUTO_OSIGURANJE (
    polisa_id             NUMBER          PRIMARY KEY,
    vozilo_id             NUMBER          NOT NULL,
    bonus_malus_klasa     VARCHAR2(5),
    teritorijalno_vazenje VARCHAR2(20)    DEFAULT 'SRBIJA'
                          CONSTRAINT chk_ao_ter CHECK (teritorijalno_vazenje IN ('SRBIJA','REGION','EVROPA','SVET')),
    CONSTRAINT fk_ao_polisa FOREIGN KEY (polisa_id)
        REFERENCES POLISA(polisa_id) ON DELETE CASCADE,
    CONSTRAINT fk_ao_vozilo FOREIGN KEY (vozilo_id)
        REFERENCES VOZILO(vozilo_id)
);

CREATE TABLE VOZAC_POLISE (
    polisa_id         NUMBER          NOT NULL,
    klijent_id        NUMBER          NOT NULL,
    PRIMARY KEY (polisa_id, klijent_id),
    CONSTRAINT fk_vp_polisa FOREIGN KEY (polisa_id)
        REFERENCES AUTO_OSIGURANJE(polisa_id) ON DELETE CASCADE,
    CONSTRAINT fk_vp_klijent FOREIGN KEY (klijent_id)
        REFERENCES KLIJENT(klijent_id)
);

CREATE TABLE NEKRETNINA (
    nekretnina_id       NUMBER          DEFAULT NEKRETNINA_ID_SEQ.NEXTVAL PRIMARY KEY,
    adresa              VARCHAR2(300)   NOT NULL,
    tip_objekta         VARCHAR2(30)    NOT NULL
                        CONSTRAINT chk_nek_tip CHECK (tip_objekta IN ('STAN','KUCA','POSLOVNI_PROSTOR','MAGACIN','ZEMLJISTE','OSTALO')),
    povrsina            NUMBER(10,2),
    godina_izgradnje    NUMBER(4),
    procenjena_vrednost NUMBER(15,2)
);

CREATE TABLE IMOVINSKO_OSIGURANJE (
    polisa_id         NUMBER          PRIMARY KEY,
    vrste_rizika      VARCHAR2(500),
    CONSTRAINT fk_imov_polisa FOREIGN KEY (polisa_id)
        REFERENCES POLISA(polisa_id) ON DELETE CASCADE
);

CREATE TABLE POLISA_NEKRETNINA (
    polisa_id         NUMBER          NOT NULL,
    nekretnina_id     NUMBER          NOT NULL,
    PRIMARY KEY (polisa_id, nekretnina_id),
    CONSTRAINT fk_pn_polisa FOREIGN KEY (polisa_id)
        REFERENCES IMOVINSKO_OSIGURANJE(polisa_id) ON DELETE CASCADE,
    CONSTRAINT fk_pn_nekretnina FOREIGN KEY (nekretnina_id)
        REFERENCES NEKRETNINA(nekretnina_id)
);

CREATE TABLE PUTOVANJE (
    polisa_id         NUMBER          PRIMARY KEY,
    destinacije       VARCHAR2(500)   NOT NULL,
    datum_polaska     DATE            NOT NULL,
    datum_povratka    DATE            NOT NULL,
    CONSTRAINT fk_put_polisa FOREIGN KEY (polisa_id)
        REFERENCES POLISA(polisa_id) ON DELETE CASCADE,
    CONSTRAINT chk_put_datumi CHECK (datum_povratka >= datum_polaska)
);

CREATE TABLE PUTOVANJE_LICE (
    polisa_id         NUMBER          NOT NULL,
    klijent_id        NUMBER          NOT NULL,
    PRIMARY KEY (polisa_id, klijent_id),
    CONSTRAINT fk_putl_putovanje FOREIGN KEY (polisa_id)
        REFERENCES PUTOVANJE(polisa_id) ON DELETE CASCADE,
    CONSTRAINT fk_putl_klijent FOREIGN KEY (klijent_id)
        REFERENCES KLIJENT(klijent_id)
);

CREATE TABLE DODATNO_POKRICE (
    pokrice_id        NUMBER          DEFAULT POKRICE_ID_SEQ.NEXTVAL PRIMARY KEY,
    polisa_id         NUMBER          NOT NULL,
    naziv             VARCHAR2(150)   NOT NULL,
    opis              VARCHAR2(500),
    -- Promenjen naziv kolone: limit_pokrica (bez specijalnog znaka)
    -- u mapiranju je: Map(x => x.LimitPokrića).Column("LIMIT_POKRICA")
    limit_pokrica     NUMBER(15,2),
    fransiza          NUMBER(15,2)    DEFAULT 0,
    dodatna_premija   NUMBER(15,2)    NOT NULL
                      CONSTRAINT chk_dp_prem CHECK (dodatna_premija >= 0),
    CONSTRAINT fk_dp_polisa FOREIGN KEY (polisa_id)
        REFERENCES POLISA(polisa_id) ON DELETE CASCADE
);

CREATE TABLE ISTORIJA_POLISE (
    istorija_id       NUMBER          DEFAULT ISTORIJA_ID_SEQ.NEXTVAL PRIMARY KEY,
    polisa_id         NUMBER          NOT NULL,
    tip_promene       VARCHAR2(20)    NOT NULL
                      CONSTRAINT chk_ip_tip CHECK (tip_promene IN ('IZMENA','OBNOVA','RASKID','MIROVANJE','REAKTIVACIJA')),
    datum_promene     DATE            DEFAULT SYSDATE NOT NULL,
    opis                    VARCHAR2(500),
    korisnik_osoblje_id     NUMBER,
    CONSTRAINT fk_ip_polisa   FOREIGN KEY (polisa_id)
        REFERENCES POLISA(polisa_id) ON DELETE CASCADE,
    CONSTRAINT fk_ip_korisnik FOREIGN KEY (korisnik_osoblje_id)
        REFERENCES OSOBLJE(osoblje_id)
);

-- ------------------------------------------------------------
-- 3.4 STETE
-- ------------------------------------------------------------

CREATE TABLE STETA (
    steta_id          NUMBER          DEFAULT STETA_ID_SEQ.NEXTVAL PRIMARY KEY,
    broj_stete        VARCHAR2(30)    UNIQUE NOT NULL,
    datum_prijave     DATE            DEFAULT SYSDATE NOT NULL,
    datum_nastanka    DATE            NOT NULL,
    polisa_id         NUMBER          NOT NULL,
    podnosilac_id     NUMBER          NOT NULL,
    vrsta_stete       VARCHAR2(30)    NOT NULL
                      CONSTRAINT chk_st_vrsta CHECK (vrsta_stete IN ('AUTO','ZDRAVSTVENA','IMOVINSKA','PUTNA','ZIVOTNA','OSTALO')),
    opis_dogadjaja    VARCHAR2(2000),
    lokacija          VARCHAR2(300),
    status            VARCHAR2(20)    DEFAULT 'PRIJAVLJENA'
                      CONSTRAINT chk_st_status CHECK (status IN ('PRIJAVLJENA','U_OBRADI','U_PROCENI','ODOBRENA','ODBIJENA','ISPLACENA','ZATVORENA')),
    procenjeni_iznos  NUMBER(15,2),
    agent_id          NUMBER,
    CONSTRAINT fk_st_polisa FOREIGN KEY (polisa_id)
        REFERENCES POLISA(polisa_id),
    CONSTRAINT fk_st_podnosilac FOREIGN KEY (podnosilac_id)
        REFERENCES KLIJENT(klijent_id),
    CONSTRAINT fk_st_agent FOREIGN KEY (agent_id)
        REFERENCES AGENT(osoblje_id),
    CONSTRAINT chk_st_datumi CHECK (datum_prijave >= datum_nastanka)
);

CREATE TABLE AUTO_STETA (
    steta_id          NUMBER          PRIMARY KEY,
    zapisnik_policije VARCHAR2(50),
    servis_id         VARCHAR2(100),
    vozilo_id         NUMBER,
    CONSTRAINT fk_ast_steta FOREIGN KEY (steta_id)
        REFERENCES STETA(steta_id) ON DELETE CASCADE,
    CONSTRAINT fk_ast_vozilo FOREIGN KEY (vozilo_id)
        REFERENCES VOZILO(vozilo_id)
);

CREATE TABLE ZDRAVSTVENA_STETA (
    steta_id                  NUMBER          PRIMARY KEY,
    dijagnoza                 VARCHAR2(200),
    medicinska_dokumentacija  VARCHAR2(500),
    zdravstvena_ustanova      VARCHAR2(200),
    lekar_id                  NUMBER,
    CONSTRAINT fk_zdst_steta FOREIGN KEY (steta_id)
        REFERENCES STETA(steta_id) ON DELETE CASCADE,
    CONSTRAINT fk_zdst_lekar FOREIGN KEY (lekar_id)
        REFERENCES LEKAR(osoblje_id)
);

CREATE TABLE IMOVINSKA_STETA (
    steta_id          NUMBER          PRIMARY KEY,
    procena_ostecenja VARCHAR2(500),
    izvodjac_sanacije VARCHAR2(200),
    CONSTRAINT fk_imst_steta FOREIGN KEY (steta_id)
        REFERENCES STETA(steta_id) ON DELETE CASCADE
);

CREATE TABLE OSTECENI_PREDMET (
    osteceni_predmet_id NUMBER        DEFAULT OSTECENIPREDMET_ID_SEQ.NEXTVAL PRIMARY KEY,
    steta_id          NUMBER          NOT NULL,
    tip_predmeta      VARCHAR2(50),
    opis_ostecenja    VARCHAR2(500),
    procenjeni_iznos  NUMBER(15,2),
    CONSTRAINT fk_op_steta FOREIGN KEY (steta_id)
        REFERENCES STETA(steta_id) ON DELETE CASCADE
);

CREATE TABLE OSTECENO_LICE (
    osteceno_lice_id  NUMBER          DEFAULT OSTECENOLICE_ID_SEQ.NEXTVAL PRIMARY KEY,
    steta_id          NUMBER          NOT NULL,
    klijent_id        NUMBER,
    ime_prezime       VARCHAR2(200),
    opis_povrede      VARCHAR2(500),
    iznos_naknade     NUMBER(15,2),
    CONSTRAINT fk_ol_steta FOREIGN KEY (steta_id)
        REFERENCES STETA(steta_id) ON DELETE CASCADE,
    CONSTRAINT fk_ol_klijent FOREIGN KEY (klijent_id)
        REFERENCES KLIJENT(klijent_id)
);

CREATE TABLE FAZA_OBRADE (
    faza_id           NUMBER          DEFAULT FAZA_ID_SEQ.NEXTVAL PRIMARY KEY,
    steta_id          NUMBER          NOT NULL,
    redni_broj_faze   NUMBER          NOT NULL,
    naziv_faze        VARCHAR2(100)   NOT NULL,
    datum_pocetka     DATE            NOT NULL,
    datum_zavrsetka   DATE,
    odgovorno_lice_id NUMBER,
    odluka            VARCHAR2(20)
                      CONSTRAINT chk_fo_odluka CHECK (odluka IN ('ODOBRENA','ODBIJENA','POTREBNA_DOPUNA','U_TOKU',NULL)),
    dokumentacija     VARCHAR2(1000),
    napomena          VARCHAR2(500),
    CONSTRAINT fk_fo_steta FOREIGN KEY (steta_id)
        REFERENCES STETA(steta_id) ON DELETE CASCADE,
    CONSTRAINT fk_fo_osoblje FOREIGN KEY (odgovorno_lice_id)
        REFERENCES OSOBLJE(osoblje_id),
    CONSTRAINT uq_fo_steta_faza UNIQUE (steta_id, redni_broj_faze)
);

CREATE TABLE PROCENA_STETE (
    procena_id        NUMBER          DEFAULT PROCENA_ID_SEQ.NEXTVAL PRIMARY KEY,
    steta_id          NUMBER          NOT NULL,
    datum_procene     DATE            NOT NULL,
    procenitelj_id    NUMBER          NOT NULL,
    metod_procene     VARCHAR2(100),
    nalaz             VARCHAR2(2000),
    procenjeni_iznos  NUMBER(15,2)    NOT NULL
                      CONSTRAINT chk_ps_iznos CHECK (procenjeni_iznos >= 0),
    preporuka         VARCHAR2(500),
    CONSTRAINT fk_ps_steta FOREIGN KEY (steta_id)
        REFERENCES STETA(steta_id) ON DELETE CASCADE,
    CONSTRAINT fk_ps_procenitelj FOREIGN KEY (procenitelj_id)
        REFERENCES PROCENITELJ(osoblje_id)
);

-- ============================================================
-- SEKCIJA 4: INDEKSI
-- ============================================================

CREATE INDEX idx_polisa_tip      ON POLISA(tip_osiguranja);
CREATE INDEX idx_polisa_status   ON POLISA(status);
CREATE INDEX idx_polisa_agent    ON POLISA(agent_id);
CREATE INDEX idx_polisa_ugovarac ON POLISA(ugovarac_id);
CREATE INDEX idx_steta_polisa    ON STETA(polisa_id);
CREATE INDEX idx_steta_status    ON STETA(status);
CREATE INDEX idx_steta_vrsta     ON STETA(vrsta_stete);
CREATE INDEX idx_klijent_tip     ON KLIJENT(tip_klijenta);
CREATE INDEX idx_vozilo_reg      ON VOZILO(registracija);
CREATE INDEX idx_faza_steta      ON FAZA_OBRADE(steta_id);

-- ============================================================
-- SEKCIJA 5: KORISNI VIEW-OVI
-- ============================================================

CREATE OR REPLACE VIEW V_AKTIVNE_POLISE AS
SELECT
    p.polisa_id, p.broj_polise, p.tip_osiguranja,
    p.datum_pocetka, p.datum_isteka, p.osnovna_premija,
    p.valuta, p.status,
    k.naziv        AS ugovarac,
    k.telefon      AS ugovarac_telefon,
    o.ime || ' ' || o.prezime AS agent,
    ROUND(p.datum_isteka - SYSDATE) AS dana_do_isteka
FROM POLISA p
JOIN KLIJENT k ON k.klijent_id = p.ugovarac_id
LEFT JOIN AGENT a  ON a.osoblje_id = p.agent_id
LEFT JOIN OSOBLJE o ON o.osoblje_id = a.osoblje_id
WHERE p.status = 'AKTIVNA';

CREATE OR REPLACE VIEW V_STATISTIKA_STETA AS
SELECT
    vrsta_stete, status,
    COUNT(*)                       AS broj_steta,
    NVL(SUM(procenjeni_iznos), 0)  AS ukupan_iznos,
    NVL(AVG(procenjeni_iznos), 0)  AS prosecni_iznos
FROM STETA
GROUP BY vrsta_stete, status
ORDER BY vrsta_stete, status;

CREATE OR REPLACE VIEW V_AGENTI_PREGLED AS
SELECT
    o.osoblje_id,
    o.ime || ' ' || o.prezime AS ime_agenta,
    a.tip_agenta, a.region_rada, a.licenca,
    a.provizija_procenat,
    COUNT(p.polisa_id)              AS broj_polisa,
    NVL(SUM(p.osnovna_premija), 0)  AS ukupna_vrednost
FROM OSOBLJE o
JOIN AGENT a ON a.osoblje_id = o.osoblje_id
LEFT JOIN POLISA p ON p.agent_id = a.osoblje_id AND p.status = 'AKTIVNA'
GROUP BY o.osoblje_id, o.ime, o.prezime, a.tip_agenta, a.region_rada, a.licenca, a.provizija_procenat
ORDER BY ukupna_vrednost DESC NULLS LAST;

CREATE OR REPLACE VIEW V_POLISE_ISTICU AS
SELECT
    p.polisa_id, p.broj_polise, p.tip_osiguranja,
    k.naziv AS ugovarac, k.telefon, k.email,
    p.datum_isteka,
    ROUND(p.datum_isteka - SYSDATE) AS dana_do_isteka
FROM POLISA p
JOIN KLIJENT k ON k.klijent_id = p.ugovarac_id
WHERE p.status = 'AKTIVNA'
  AND p.datum_isteka BETWEEN SYSDATE AND SYSDATE + 30
ORDER BY p.datum_isteka;

-- ============================================================
-- SEKCIJA 6: SAMPLE PODACI
-- ============================================================

-- Klijenti - fizicka lica
INSERT INTO KLIJENT (naziv, adresa, telefon, email, status, tip_klijenta)
VALUES ('Petar Petrovic', 'Bulevar oslobodjenja 15, Nis', '064-111-2233', 'petar.petrovic@gmail.com', 'AKTIVAN', 'FIZICKO_LICE');

INSERT INTO KLIJENT (naziv, adresa, telefon, email, status, tip_klijenta)
VALUES ('Milica Jovanovic', 'Cara Dusana 8, Beograd', '063-444-5566', 'milica.jov@yahoo.com', 'AKTIVAN', 'FIZICKO_LICE');

INSERT INTO KLIJENT (naziv, adresa, telefon, email, status, tip_klijenta)
VALUES ('Nikola Markovic', 'Svetozara Markovica 22, Kragujevac', '065-777-8899', 'n.markovic@mail.com', 'AKTIVAN', 'FIZICKO_LICE');

INSERT INTO FIZICKO_LICE VALUES (KLIJENT_ID_SEQ.CURRVAL - 2, '1501985710011', DATE '1985-01-15', 'Inzenjer');
INSERT INTO FIZICKO_LICE VALUES (KLIJENT_ID_SEQ.CURRVAL - 1, '2203990725022', DATE '1990-03-22', 'Lekar');
INSERT INTO FIZICKO_LICE VALUES (KLIJENT_ID_SEQ.CURRVAL,     '0809978710033', DATE '1978-09-08', 'Profesor');

-- Klijent - pravno lice
INSERT INTO KLIJENT (naziv, adresa, telefon, email, status, tip_klijenta)
VALUES ('TechSolutions d.o.o.', 'Bulevar Nikole Tesle 5, Nis', '018-555-1234', 'office@techsol.rs', 'AKTIVAN', 'PRAVNO_LICE');

INSERT INTO PRAVNO_LICE VALUES (KLIJENT_ID_SEQ.CURRVAL, '100123456', '12345678', 'Razvoj softvera');

COMMIT;

-- Osoblje - agent
INSERT INTO OSOBLJE (ime, prezime, jmbg, adresa, telefon, email, datum_angazovanja, status, tip_osoblja)
VALUES ('Marko', 'Dimitrijevic', '1503985710066', 'Partizanska 12, Nis', '064-300-1122', 'm.dimit@osig.rs', DATE '2018-03-01', 'AKTIVAN', 'AGENT');

INSERT INTO AGENT VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'INTERNI', 'AG-NIS-001', 'Nis i okolina', 7.50);

-- Osoblje - procenitelj
INSERT INTO OSOBLJE (ime, prezime, jmbg, adresa, telefon, email, datum_angazovanja, status, tip_osoblja)
VALUES ('Zoran', 'Stankovic', '1204975710088', 'Cara Lazara 7, Kragujevac', '065-500-3344', 'z.stan@osig.rs', DATE '2015-01-10', 'AKTIVAN', 'PROCENITELJ');

INSERT INTO PROCENITELJ VALUES (OSOBLJE_ID_SEQ.CURRVAL);
INSERT INTO OBLAST_PROCENE (osoblje_id, oblast) VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'VOZILO');
INSERT INTO OBLAST_PROCENE (osoblje_id, oblast) VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'IMOVINA');

-- Osoblje - lekar
INSERT INTO OSOBLJE (ime, prezime, jmbg, adresa, telefon, email, datum_angazovanja, status, tip_osoblja)
VALUES ('Ana', 'Ristic', '3101988725099', 'Zeleni bulevar 22, Nis', '060-600-4455', 'a.ristic@osig.rs', DATE '2019-09-01', 'AKTIVAN', 'LEKAR');

INSERT INTO LEKAR VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'Opsta medicina', 'LIC-MED-001');

COMMIT;

-- Vozilo
INSERT INTO VOZILO (registracija, marka, model, godina_proizvodnje, vlasnik_id)
VALUES ('NI-123-AB', 'Volkswagen', 'Golf 7', 2018, 1);

-- Polisa - auto
INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-AUTO-2025-001', 'AUTO', DATE '2025-01-01', DATE '2026-01-01',
    35000, 'RSD', 'GODISNJE', 1, 1);

INSERT INTO AUTO_OSIGURANJE VALUES (POLISA_ID_SEQ.CURRVAL, VOZILO_ID_SEQ.CURRVAL, 'B5', 'SRBIJA');

-- Polisa - zdravstveno
INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-ZDRA-2025-001', 'ZDRAVSTVENO', DATE '2025-01-01', DATE '2026-01-01',
    8000, 'RSD', 'MESECNO', 1, 3);

INSERT INTO ZDRAVSTVENO_OSIGURANJE VALUES (POLISA_ID_SEQ.CURRVAL,
    'Klinicki centar Nis', 150000, 80000, 500000, 8000);

COMMIT;

-- Steta
INSERT INTO STETA (broj_stete, datum_nastanka, polisa_id, podnosilac_id,
    vrsta_stete, opis_dogadjaja, lokacija, status, procenjeni_iznos, agent_id)
VALUES ('STE-AUT-2025-001', DATE '2025-03-15',
    (SELECT polisa_id FROM POLISA WHERE broj_polise = 'POL-AUTO-2025-001'),
    1, 'AUTO', 'Sudarna nezgoda na raskrsnici', 'Bulevar JNA, Nis',
    'U_PROCENI', 85000, 1);

INSERT INTO AUTO_STETA VALUES (STETA_ID_SEQ.CURRVAL, 'ZAP-NI-2025-001', NULL, VOZILO_ID_SEQ.CURRVAL);

INSERT INTO FAZA_OBRADE (steta_id, redni_broj_faze, naziv_faze, datum_pocetka,
    odgovorno_lice_id, odluka, dokumentacija)
VALUES (STETA_ID_SEQ.CURRVAL, 1, 'Prijem prijave', DATE '2025-03-15', 1, 'ODOBRENA', 'Prijavni formular');

COMMIT;

-- ------------------------------------------------------------
-- Dodatni sample podaci (Klijenti, Osoblje, Polise, Stete)
-- ------------------------------------------------------------

-- ---- Klijenti: fizicka lica ----
INSERT INTO KLIJENT (naziv, adresa, telefon, email, status, tip_klijenta)
VALUES ('Jovana Ilic', 'Knez Mihailova 44, Beograd', '064-222-3344', 'jovana.ilic@gmail.com', 'AKTIVAN', 'FIZICKO_LICE');
INSERT INTO FIZICKO_LICE VALUES (KLIJENT_ID_SEQ.CURRVAL, '1002992710099', DATE '1992-01-10', 'Advokat');

INSERT INTO KLIJENT (naziv, adresa, telefon, email, status, tip_klijenta)
VALUES ('Stefan Nikolic', 'Vojvode Stepe 55, Nis', '065-333-4455', 'stefan.nikolic@yahoo.com', 'AKTIVAN', 'FIZICKO_LICE');
INSERT INTO FIZICKO_LICE VALUES (KLIJENT_ID_SEQ.CURRVAL, '0511987710077', DATE '1987-11-05', 'IT Konsultant');

INSERT INTO KLIJENT (naziv, adresa, telefon, email, status, tip_klijenta)
VALUES ('Ana Kovacevic', 'Kralja Petra 8, Novi Sad', '063-444-5522', 'ana.kovacevic@mail.com', 'AKTIVAN', 'FIZICKO_LICE');
INSERT INTO FIZICKO_LICE VALUES (KLIJENT_ID_SEQ.CURRVAL, '2207995710055', DATE '1995-07-22', 'Nastavnik');

-- ---- Klijenti: pravna lica ----
INSERT INTO KLIJENT (naziv, adresa, telefon, email, status, tip_klijenta)
VALUES ('AutoServis Beograd d.o.o.', 'Zorana Djindjica 100, Beograd', '011-333-2211', 'office@autoservisbg.rs', 'AKTIVAN', 'PRAVNO_LICE');
INSERT INTO PRAVNO_LICE VALUES (KLIJENT_ID_SEQ.CURRVAL, '100223344', '20334455', 'Servis vozila');
INSERT INTO KONTAKT_OSOBA (klijent_id, ime, prezime, telefon, email, funkcija)
VALUES (KLIJENT_ID_SEQ.CURRVAL, 'Dragan', 'Mitic', '064-100-2000', 'dragan.mitic@autoservisbg.rs', 'Direktor');

INSERT INTO KLIJENT (naziv, adresa, telefon, email, status, tip_klijenta)
VALUES ('MediPlus Klinika d.o.o.', 'Bulevar Oslobodjenja 200, Nis', '018-400-5566', 'info@mediplus.rs', 'AKTIVAN', 'PRAVNO_LICE');
INSERT INTO PRAVNO_LICE VALUES (KLIJENT_ID_SEQ.CURRVAL, '100556677', '30556677', 'Zdravstvene usluge');
INSERT INTO KONTAKT_OSOBA (klijent_id, ime, prezime, telefon, email, funkcija)
VALUES (KLIJENT_ID_SEQ.CURRVAL, 'Milica', 'Radovic', '064-200-3000', 'milica.radovic@mediplus.rs', 'Menadzer');

-- ---- Klijenti: javna institucija ----
INSERT INTO KLIJENT (naziv, adresa, telefon, email, status, tip_klijenta)
VALUES ('Opstina Nis', 'Trg kralja Milana 1, Nis', '018-501-000', 'info@nis.rs', 'AKTIVAN', 'JAVNA_INSTITUCIJA');
INSERT INTO JAVNA_INSTITUCIJA VALUES (KLIJENT_ID_SEQ.CURRVAL, '100778899', '40778899', 'Lokalna samouprava', 'OPSTINSKA');
INSERT INTO KONTAKT_OSOBA (klijent_id, ime, prezime, telefon, email, funkcija)
VALUES (KLIJENT_ID_SEQ.CURRVAL, 'Vladimir', 'Jankovic', '064-300-4000', 'vladimir.jankovic@nis.rs', 'Sekretar');

COMMIT;

-- ---- Osoblje: agenti ----
INSERT INTO OSOBLJE (ime, prezime, jmbg, adresa, telefon, email, datum_angazovanja, status, tip_osoblja)
VALUES ('Jelena', 'Petrovic', '1509990710011', 'Njegoseva 5, Beograd', '064-500-1111', 'j.petrovic@osig.rs', DATE '2019-04-15', 'AKTIVAN', 'AGENT');
INSERT INTO AGENT VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'INTERNI', 'AG-BG-002', 'Beograd i okolina', 6.00);

INSERT INTO OSOBLJE (ime, prezime, jmbg, adresa, telefon, email, datum_angazovanja, status, tip_osoblja)
VALUES ('Nemanja', 'Djordjevic', '2108988710022', 'Bulevar Evrope 10, Novi Sad', '065-600-2222', 'n.djordjevic@osig.rs', DATE '2020-06-01', 'AKTIVAN', 'AGENT');
INSERT INTO AGENT VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'EKSTERNI', 'AG-NS-003', 'Novi Sad i Vojvodina', 8.50);

-- ---- Osoblje: procenitelj ----
INSERT INTO OSOBLJE (ime, prezime, jmbg, adresa, telefon, email, datum_angazovanja, status, tip_osoblja)
VALUES ('Milan', 'Vasic', '0312982710033', 'Cara Dusana 15, Krusevac', '066-700-3333', 'm.vasic@osig.rs', DATE '2017-09-10', 'AKTIVAN', 'PROCENITELJ');
INSERT INTO PROCENITELJ VALUES (OSOBLJE_ID_SEQ.CURRVAL);
INSERT INTO OBLAST_PROCENE (osoblje_id, oblast) VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'ZDRAVSTVO');
INSERT INTO OBLAST_PROCENE (osoblje_id, oblast) VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'SPECIJALNE_STETE');

-- ---- Osoblje: lekar ----
INSERT INTO OSOBLJE (ime, prezime, jmbg, adresa, telefon, email, datum_angazovanja, status, tip_osoblja)
VALUES ('Snezana', 'Popovic', '1409985710044', 'Kosovke devojke 3, Nis', '060-800-4444', 's.popovic@osig.rs', DATE '2021-02-20', 'AKTIVAN', 'LEKAR');
INSERT INTO LEKAR VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'Ortopedija', 'LIC-MED-002');

-- ---- Osoblje: pravnici ----
INSERT INTO OSOBLJE (ime, prezime, jmbg, adresa, telefon, email, datum_angazovanja, status, tip_osoblja)
VALUES ('Aleksandar', 'Jovanovic', '0805980710055', 'Generala Milojka Lesjanina 2, Nis', '063-900-5555', 'a.jovanovic@osig.rs', DATE '2016-03-01', 'AKTIVAN', 'PRAVNIK');
INSERT INTO PRAVNIK VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'INTERNI', 'BAR-2016-123');

INSERT INTO OSOBLJE (ime, prezime, jmbg, adresa, telefon, email, datum_angazovanja, status, tip_osoblja)
VALUES ('Milica', 'Stankovic', '2711983710066', 'Balkanska 20, Beograd', '064-111-6666', 'm.stankovic@osig.rs', DATE '2022-01-05', 'AKTIVAN', 'PRAVNIK');
INSERT INTO PRAVNIK VALUES (OSOBLJE_ID_SEQ.CURRVAL, 'EKSTERNI', 'BAR-2022-456');

COMMIT;

-- ---- Vozila ----
INSERT INTO VOZILO (registracija, marka, model, godina_proizvodnje, vlasnik_id)
VALUES ('BG-456-CD', 'Toyota', 'Corolla', 2020, (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Jovana Ilic'));

INSERT INTO VOZILO (registracija, marka, model, godina_proizvodnje, vlasnik_id)
VALUES ('NI-789-EF', 'Skoda', 'Octavia', 2019, (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Stefan Nikolic'));

INSERT INTO VOZILO (registracija, marka, model, godina_proizvodnje, vlasnik_id)
VALUES ('NS-321-GH', 'Renault', 'Clio', 2021, (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Ana Kovacevic'));

-- ---- Nekretnine ----
INSERT INTO NEKRETNINA (adresa, tip_objekta, povrsina, godina_izgradnje, procenjena_vrednost)
VALUES ('Bulevar Nikole Tesle 5, Nis', 'POSLOVNI_PROSTOR', 250, 2015, 15000000);

INSERT INTO NEKRETNINA (adresa, tip_objekta, povrsina, godina_izgradnje, procenjena_vrednost)
VALUES ('Cara Dusana 8, Beograd', 'STAN', 65, 2005, 9000000);

INSERT INTO NEKRETNINA (adresa, tip_objekta, povrsina, godina_izgradnje, procenjena_vrednost)
VALUES ('Svetozara Markovica 22, Kragujevac', 'KUCA', 140, 1998, 12000000);

COMMIT;

-- ---- Polise: auto (2 nove) ----
INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-AUTO-2025-002', 'AUTO', DATE '2025-02-01', DATE '2026-02-01',
    28000, 'RSD', 'GODISNJE',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-BG-002'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Jovana Ilic'));
INSERT INTO AUTO_OSIGURANJE VALUES (POLISA_ID_SEQ.CURRVAL,
    (SELECT vozilo_id FROM VOZILO WHERE registracija = 'BG-456-CD'), 'B3', 'SRBIJA');

INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-AUTO-2025-003', 'AUTO', DATE '2025-03-10', DATE '2026-03-10',
    32000, 'RSD', 'MESECNO',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-NS-003'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Ana Kovacevic'));
INSERT INTO AUTO_OSIGURANJE VALUES (POLISA_ID_SEQ.CURRVAL,
    (SELECT vozilo_id FROM VOZILO WHERE registracija = 'NS-321-GH'), 'B7', 'EVROPA');

-- ---- Polise: zdravstveno (1 nova) ----
INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-ZDRA-2025-002', 'ZDRAVSTVENO', DATE '2025-01-15', DATE '2026-01-15',
    9500, 'RSD', 'MESECNO',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-NIS-001'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Stefan Nikolic'));
INSERT INTO ZDRAVSTVENO_OSIGURANJE VALUES (POLISA_ID_SEQ.CURRVAL,
    'Klinicki centar Beograd', 180000, 90000, 600000, 9000);

-- ---- Polise: zivotno (2 nove) ----
INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-ZIVO-2025-001', 'ZIVOTNO', DATE '2025-01-01', DATE '2035-01-01',
    12000, 'RSD', 'GODISNJE',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-NIS-001'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Jovana Ilic'));
INSERT INTO ZIVOTNO_OSIGURANJE VALUES (POLISA_ID_SEQ.CURRVAL, 3000000, 'JEDNOKRATNA');
INSERT INTO KORISNIK_ISPLATE (polisa_id, klijent_id, procenat_udela)
VALUES (POLISA_ID_SEQ.CURRVAL, (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Stefan Nikolic'), 100);

INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-ZIVO-2025-002', 'ZIVOTNO', DATE '2025-05-01', DATE '2040-05-01',
    18000, 'EUR', 'GODISNJE',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-BG-002'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Ana Kovacevic'));
INSERT INTO ZIVOTNO_OSIGURANJE VALUES (POLISA_ID_SEQ.CURRVAL, 5000000, 'MESECNA_RENTA');
INSERT INTO KORISNIK_ISPLATE (polisa_id, klijent_id, procenat_udela)
VALUES (POLISA_ID_SEQ.CURRVAL, (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Jovana Ilic'), 50);
INSERT INTO KORISNIK_ISPLATE (polisa_id, klijent_id, procenat_udela)
VALUES (POLISA_ID_SEQ.CURRVAL, (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Stefan Nikolic'), 50);

-- ---- Polise: imovinsko (2 nove) ----
INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-IMOV-2025-001', 'IMOVINSKO', DATE '2025-01-01', DATE '2026-01-01',
    15000, 'RSD', 'GODISNJE',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-NIS-001'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'AutoServis Beograd d.o.o.'));
INSERT INTO IMOVINSKO_OSIGURANJE VALUES (POLISA_ID_SEQ.CURRVAL, 'Pozar, poplava, kradja');
INSERT INTO POLISA_NEKRETNINA (polisa_id, nekretnina_id)
VALUES (POLISA_ID_SEQ.CURRVAL, (SELECT nekretnina_id FROM NEKRETNINA WHERE adresa = 'Bulevar Nikole Tesle 5, Nis'));

INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-IMOV-2025-002', 'IMOVINSKO', DATE '2025-02-15', DATE '2026-02-15',
    11000, 'RSD', 'KVARTAL',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-BG-002'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Milica Jovanovic'));
INSERT INTO IMOVINSKO_OSIGURANJE VALUES (POLISA_ID_SEQ.CURRVAL, 'Pozar, oluja');
INSERT INTO POLISA_NEKRETNINA (polisa_id, nekretnina_id)
VALUES (POLISA_ID_SEQ.CURRVAL, (SELECT nekretnina_id FROM NEKRETNINA WHERE adresa = 'Cara Dusana 8, Beograd'));

-- ---- Polise: putno (2 nove) ----
INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-PUTO-2025-001', 'PUTNO', DATE '2025-06-01', DATE '2025-06-15',
    3500, 'EUR', 'JEDNOKRATNO',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-NIS-001'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Nikola Markovic'));
INSERT INTO PUTOVANJE VALUES (POLISA_ID_SEQ.CURRVAL, 'Grcka - Halkidiki', DATE '2025-06-05', DATE '2025-06-14');
INSERT INTO PUTOVANJE_LICE (polisa_id, klijent_id)
VALUES (POLISA_ID_SEQ.CURRVAL, (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Nikola Markovic'));

INSERT INTO POLISA (broj_polise, tip_osiguranja, datum_pocetka, datum_isteka,
    osnovna_premija, valuta, nacin_placanja, agent_id, ugovarac_id)
VALUES ('POL-PUTO-2025-002', 'PUTNO', DATE '2025-07-01', DATE '2025-07-20',
    4200, 'EUR', 'JEDNOKRATNO',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-NS-003'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Ana Kovacevic'));
INSERT INTO PUTOVANJE VALUES (POLISA_ID_SEQ.CURRVAL, 'Italija - Rim, Milano', DATE '2025-07-02', DATE '2025-07-18');
INSERT INTO PUTOVANJE_LICE (polisa_id, klijent_id)
VALUES (POLISA_ID_SEQ.CURRVAL, (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Ana Kovacevic'));

COMMIT;

-- ---- Dodatna pokrica i istorija (na jednu postojecu polisu) ----
INSERT INTO DODATNO_POKRICE (polisa_id, naziv, opis, limit_pokrica, fransiza, dodatna_premija)
VALUES ((SELECT polisa_id FROM POLISA WHERE broj_polise = 'POL-AUTO-2025-002'),
    'Asistencija na putu', 'Sirom Evrope, 24/7', 500000, 0, 1500);

INSERT INTO ISTORIJA_POLISE (polisa_id, tip_promene, datum_promene, opis, korisnik_osoblje_id)
VALUES ((SELECT polisa_id FROM POLISA WHERE broj_polise = 'POL-AUTO-2025-002'),
    'IZMENA', DATE '2025-03-01', 'Dodata asistencija na putu',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-BG-002'));

COMMIT;

-- ---- Stete: auto (1 nova) ----
INSERT INTO STETA (broj_stete, datum_nastanka, polisa_id, podnosilac_id,
    vrsta_stete, opis_dogadjaja, lokacija, status, procenjeni_iznos, agent_id)
VALUES ('STE-AUT-2025-002', DATE '2025-04-10',
    (SELECT polisa_id FROM POLISA WHERE broj_polise = 'POL-AUTO-2025-002'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Jovana Ilic'),
    'AUTO', 'Ostecenje branika u parkingu', 'Knez Mihailova, Beograd',
    'ODOBRENA', 25000, (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-BG-002'));
INSERT INTO AUTO_STETA VALUES (STETA_ID_SEQ.CURRVAL, NULL, 'SRV-BG-2025-014',
    (SELECT vozilo_id FROM VOZILO WHERE registracija = 'BG-456-CD'));
INSERT INTO FAZA_OBRADE (steta_id, redni_broj_faze, naziv_faze, datum_pocetka, datum_zavrsetka,
    odgovorno_lice_id, odluka, dokumentacija)
VALUES (STETA_ID_SEQ.CURRVAL, 1, 'Prijem prijave', DATE '2025-04-10', DATE '2025-04-11',
    (SELECT osoblje_id FROM AGENT WHERE licenca = 'AG-BG-002'), 'ODOBRENA', 'Prijavni formular');
INSERT INTO FAZA_OBRADE (steta_id, redni_broj_faze, naziv_faze, datum_pocetka,
    odgovorno_lice_id, odluka, dokumentacija)
VALUES (STETA_ID_SEQ.CURRVAL, 2, 'Procena stete', DATE '2025-04-12',
    (SELECT osoblje_id FROM OSOBLJE WHERE ime = 'Milan' AND prezime = 'Vasic'),
    'ODOBRENA', 'Izvestaj procenitelja');
INSERT INTO PROCENA_STETE (steta_id, datum_procene, procenitelj_id, metod_procene, nalaz, procenjeni_iznos, preporuka)
VALUES (STETA_ID_SEQ.CURRVAL, DATE '2025-04-12',
    (SELECT osoblje_id FROM OSOBLJE WHERE ime = 'Milan' AND prezime = 'Vasic'),
    'Vizuelni pregled', 'Ostecenje prednjeg branika, potrebna zamena', 25000, 'Isplatiti puni iznos');

-- ---- Stete: zdravstvena (1 nova) ----
INSERT INTO STETA (broj_stete, datum_nastanka, polisa_id, podnosilac_id,
    vrsta_stete, opis_dogadjaja, lokacija, status, procenjeni_iznos)
VALUES ('STE-ZDR-2025-001', DATE '2025-03-20',
    (SELECT polisa_id FROM POLISA WHERE broj_polise = 'POL-ZDRA-2025-002'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'Stefan Nikolic'),
    'ZDRAVSTVENA', 'Hospitalizacija zbog preloma noge', 'Klinicki centar Beograd',
    'U_PROCENI', 120000);
INSERT INTO ZDRAVSTVENA_STETA VALUES (STETA_ID_SEQ.CURRVAL, 'Prelom potkolenice',
    'Otpusna lista, RTG snimak', 'Klinicki centar Beograd',
    (SELECT osoblje_id FROM OSOBLJE WHERE ime = 'Snezana' AND prezime = 'Popovic'));
INSERT INTO FAZA_OBRADE (steta_id, redni_broj_faze, naziv_faze, datum_pocetka, datum_zavrsetka,
    odluka, dokumentacija)
VALUES (STETA_ID_SEQ.CURRVAL, 1, 'Prijem prijave', DATE '2025-03-20', DATE '2025-03-21', 'ODOBRENA', 'Prijavni formular');

-- ---- Stete: imovinska (1 nova) ----
INSERT INTO STETA (broj_stete, datum_nastanka, polisa_id, podnosilac_id,
    vrsta_stete, opis_dogadjaja, lokacija, status, procenjeni_iznos)
VALUES ('STE-IMO-2025-001', DATE '2025-05-05',
    (SELECT polisa_id FROM POLISA WHERE broj_polise = 'POL-IMOV-2025-001'),
    (SELECT klijent_id FROM KLIJENT WHERE naziv = 'AutoServis Beograd d.o.o.'),
    'IMOVINSKA', 'Ostecenje krova nakon nevremena', 'Zorana Djindjica 100, Beograd',
    'U_OBRADI', 350000);
INSERT INTO IMOVINSKA_STETA VALUES (STETA_ID_SEQ.CURRVAL, 'Ostecenje krovnog pokrivaca na 40m2', 'GradnjaPro d.o.o.');
INSERT INTO OSTECENI_PREDMET (steta_id, tip_predmeta, opis_ostecenja, procenjeni_iznos)
VALUES (STETA_ID_SEQ.CURRVAL, 'Krovni pokrivac', 'Polomljeni crepovi i ostecena hidroizolacija', 350000);
INSERT INTO FAZA_OBRADE (steta_id, redni_broj_faze, naziv_faze, datum_pocetka,
    odluka, dokumentacija)
VALUES (STETA_ID_SEQ.CURRVAL, 1, 'Prijem prijave', DATE '2025-05-05', 'U_TOKU', 'Fotografije stete');

COMMIT;

-- ============================================================
-- SEKCIJA 7: VERIFIKACIJA
-- ============================================================

-- Broj tabela
SELECT COUNT(*) AS broj_tabela FROM USER_TABLES;

-- Broj sequence-ova
SELECT COUNT(*) AS broj_sekvenci FROM USER_SEQUENCES;

-- Provera da li su podaci uneseni
SELECT 'KLIJENTI'   AS tabela, COUNT(*) AS broj FROM KLIJENT   UNION ALL
SELECT 'POLISE',              COUNT(*)          FROM POLISA    UNION ALL
SELECT 'OSOBLJE',             COUNT(*)          FROM OSOBLJE   UNION ALL
SELECT 'STETE',               COUNT(*)          FROM STETA;

-- ============================================================
-- KRAJ
-- ============================================================

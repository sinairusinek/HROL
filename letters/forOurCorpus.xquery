  for $person in doc("OurCorpus3.xml")//*:persName
       return
       $person/text()
        

(:<ul>{
        for $person in collection("catalog.xml")//*:Person
       where $person//*:AuthoritativeID
       return
        <li>
        {$person/*:firstName},{$person/*:lastName},{$person//*:AuthoritativeID//*:id/text()}
        </li>
        }
</ul>:)

(: first task: <ul>{
        for $ref in collection("catalog.xml")//*:ResourceProxy/*:ResourceRef
        where not(contains($ref, "http://hdl.handle.net"))
        return
            <li>
            {$ref} is not a handle
            </li>}
    </ul>:)
    
  (: second task: <ul>{
        for $ZIPindeed in collection("catalog.xml")//*:ResourceProxy
        where $ZIPindeed/*:ResourceType/@mimetype="application/zip"
        return
        <li>
        {$ZIPindeed/*:ResourceRef/text()}
        </li>
        }
</ul>:)
(: third task
<ul>{
        for $lp in collection("catalog.xml")//*:ResourceProxy
        where $lp/*:ResourceType/text()= "LandingPage"
        return
        <li>
        {$lp}
        </li>
        }
</ul>:)
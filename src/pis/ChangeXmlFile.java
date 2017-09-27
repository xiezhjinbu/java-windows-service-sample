package pis;

import java.io.File;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.OutputKeys;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

public class ChangeXmlFile {
	public static void  readFile(File file) {
        DocumentBuilderFactory dbFactory = DocumentBuilderFactory.newInstance();
        DocumentBuilder dBuilder;
        String path=file.getPath();
        path=path.replace("\\FileReceiveHHB", "\\FileReceive");
        //System.out.println(path);
        try {
			
        	dBuilder = dbFactory.newDocumentBuilder();
            Document doc = dBuilder.parse(file);
            
            doc.getDocumentElement().normalize();
            
          //update Element value
            updateElementValue(doc);
            
          //write the updated document to file or console
            doc.getDocumentElement().normalize();
            TransformerFactory transformerFactory = TransformerFactory.newInstance();
            Transformer transformer = transformerFactory.newTransformer();
            DOMSource source = new DOMSource(doc);
            StreamResult result = new StreamResult(new File(path));
            transformer.setOutputProperty(OutputKeys.INDENT, "yes");
            transformer.transform(source, result);
            System.out.println("XML file updated successfully");
        	
        } catch (Exception e1) {
            e1.printStackTrace();
        }
	}
			
	private static void updateElementValue(Document doc) {
        NodeList PisEntBom = doc.getElementsByTagName("PisEntBomHead");
        NodeList PisEntImg = doc.getElementsByTagName("PisEntHeadType");
        NodeList PisEntDec = doc.getElementsByTagName("PisClListHeadType");
        Element emp = null;
        //loop for each employee
        if(PisEntBom.getLength()>0)
        {
        	for(int i=0; i<PisEntBom.getLength();i++){
                emp = (Element) PisEntBom.item(i);
                Node tradeCode = emp.getElementsByTagName("TradeCode").item(0).getFirstChild();
                tradeCode.setNodeValue("3312960296");
            }
        }
        if(PisEntImg.getLength()>0)
        {
        	for(int i=0; i<PisEntImg.getLength();i++){
                emp = (Element) PisEntImg.item(i);
                Node tradeCode = emp.getElementsByTagName("TradeCode").item(0).getFirstChild();
                tradeCode.setNodeValue("3312960296");
            }
        }
        if(PisEntDec.getLength()>0)
        {
        	for(int i=0; i<PisEntDec.getLength();i++){
                emp = (Element) PisEntDec.item(i);
                Node tradeCode = emp.getElementsByTagName("TradeCode").item(0).getFirstChild();
                tradeCode.setNodeValue("3312960296");
            }
        }
    }
}
